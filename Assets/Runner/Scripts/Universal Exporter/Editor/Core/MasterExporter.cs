#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if GLTFAST_INSTALLED
using GLTFast;
using GLTFast.Export;
#endif

public static class MasterExporter
{
    const string VERSION = "1.0.0";
    public static List<IExporter> GetActiveExporters()
    {
        var list = new List<IExporter>();
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
        
        foreach (var t in types)
        {
            if (typeof(IExporter).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            {
                try 
                {
                    var instance = (IExporter)Activator.CreateInstance(t);
                    list.Add(instance);
                } 
                catch (Exception e) 
                {
                    Debug.LogWarning($"[Universal Exporter] Script ignorado. Não foi possível carregar o exportador {t.Name}: {e.Message}");
                }
            }
        }
        return list.OrderBy(e => e.Order).ToList();
    }

   public static async Task RunExport(ExportScope scope, List<IExporter> selectedExporters)
    {
        string stagingDir = Path.Combine(Application.temporaryCachePath, "UniversalExport_Staging");
        if (Directory.Exists(stagingDir)) Directory.Delete(stagingDir, true);
        Directory.CreateDirectory(stagingDir);

        var context = new ExportContext(scope, stagingDir);
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        var originalScenePath = EditorSceneManager.GetActiveScene().path;

        try
        {
            var projectExporters = selectedExporters.Where(e => e.Order < 50).OrderBy(e => e.Order).ToList();
            foreach (var exp in projectExporters)
            {
                Progress(0.1f, $"Projeto: {exp.ModuleName}...");
                await exp.ExportProject(context);
            }
            
            var sceneExporters = selectedExporters.Where(e => e.Order >= 50).OrderBy(e => e.Order).ToList();
            var scenesToExport = new List<string>();

            if (scope == ExportScope.CurrentScene)
            {
                scenesToExport.Add(originalScenePath);
            }
            else
            {
                foreach (var s in EditorBuildSettings.scenes)
                    if (s.enabled) scenesToExport.Add(s.path);
            }
            
            float step = 0.7f / Mathf.Max(1, scenesToExport.Count);
            for (int i = 0; i < scenesToExport.Count; i++)
            {
                var path = scenesToExport[i];
                if (string.IsNullOrEmpty(path)) continue;
                
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Progress(0.2f + (i * step), $"Processando Cena: {scene.name}...");
                
                await RunSceneExporters(sceneExporters, scene, context);
            }
            
            Progress(0.85f, "Resolvendo dependências e copiando assets...");
            context.Assets.ResolveDependencies();
            context.Assets.CopyAllTo(stagingDir);

            WriteManifest(context, timestamp, scope);

            Progress(0.95f, "Gerando pacotes finais...");
            string zipPath = EditorUtility.SaveFilePanel("Salvar Exportação", "", $"Export_{timestamp}", "zip");
            
            if (!string.IsNullOrEmpty(zipPath))
            {
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(stagingDir, zipPath);
                
                string htmlPath = zipPath.Replace(".zip", ".html");
                GenerateHtmlReport(stagingDir, htmlPath);

                Debug.Log($"[Universal Exporter] Exportação completa! {scenesToExport.Count} cenas processadas.");
                EditorUtility.RevealInFinder(zipPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Universal Exporter] Erro: {e.Message}");
        }
        finally
        {
            if (EditorSceneManager.GetActiveScene().path != originalScenePath)
                EditorSceneManager.OpenScene(originalScenePath);
                
            EditorUtility.ClearProgressBar();
        }
    }
    
    static async Task RunSceneExporters(List<IExporter> exporters, Scene scene, ExportContext ctx)
    {
        if (!string.IsNullOrEmpty(scene.path)) ctx.TrackAsset(scene.path);

        foreach (var exporter in exporters)
        {
            Progress(0.3f, $"Cena {scene.name}: {exporter.ModuleName}...");
            await exporter.ExportScene(scene, ctx);
        }
    }
    
    static void WriteManifest(ExportContext ctx, string timestamp, ExportScope scope)
    {
        bool hasGltFast = false;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name.IndexOf("gltfast", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                hasGltFast = true;
                break;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"projectName\": \"{ExportUtils.Esc(PlayerSettings.productName)}\",");
        sb.AppendLine($"  \"exportVersion\": \"{VERSION}\",");
        sb.AppendLine($"  \"unityVersion\": \"{Application.unityVersion}\",");
        sb.AppendLine($"  \"scope\": \"{scope}\",");
        sb.AppendLine($"  \"exportedAt\": \"{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}\",");
        sb.AppendLine($"  \"gltfastInstalled\": {ExportUtils.B(hasGltFast)},");
        
        sb.AppendLine($"  \"rawAssets\": {{ \"registered\": {ctx.Assets.Registered}, \"copied\": {ctx.Assets.Copied}, \"skipped\": {ctx.Assets.Skipped} }}");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.StagingDir, "export_manifest.json"), sb.ToString(), Encoding.UTF8);
    }
    
    static void GenerateHtmlReport(string stagingDir, string finalHtmlPath)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("unity-inspector");
        if (guids.Length == 0)
        {
            Debug.LogError("[Universal Exporter] Template 'unity-inspector.html' não encontrado no projeto! O relatório não foi gerado.");
            return;
        }

        string templatePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
        string htmlContent = File.ReadAllText(templatePath);
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<script>");
        sb.AppendLine("const EXPORT_DATA = { files: [");

        string[] jsonFiles = Directory.GetFiles(stagingDir, "*.json", SearchOption.AllDirectories);
        for (int i = 0; i < jsonFiles.Length; i++)
        {
            string fileName = Path.GetFileName(jsonFiles[i]);
            byte[] fileBytes = File.ReadAllBytes(jsonFiles[i]);
            string base64 = System.Convert.ToBase64String(fileBytes);
            
            string comma = i < jsonFiles.Length - 1 ? "," : "";
            sb.AppendLine($"  {{ name: \"{fileName}\", content: \"{base64}\" }}{comma}");
        }

        sb.AppendLine("] };");
        sb.AppendLine("</script>");

        if (htmlContent.Contains("</head>"))
        {
            htmlContent = htmlContent.Replace("</head>", sb.ToString() + "</head>");
        }
        else
        {
            htmlContent += sb.ToString();
        }
        
        File.WriteAllText(finalHtmlPath, htmlContent, System.Text.Encoding.UTF8);
    }

    static void Progress(float t, string msg) => EditorUtility.DisplayProgressBar("Universal Exporter", msg, t);
}
#endif