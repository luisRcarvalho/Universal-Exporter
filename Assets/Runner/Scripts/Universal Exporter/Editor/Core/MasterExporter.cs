#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class MasterExporter
{
    const string VERSION = "1.0.0";
    
    private static void Progress(float p, string msg)
    {
        EditorUtility.DisplayProgressBar("Universal Exporter", msg, p);
    }

    public static async Task RunExport(ExportScope scope, List<string> scenesToExport, List<IExporter> selectedExporters)
    {
        string stagingDir = Path.Combine(Application.temporaryCachePath, "UniversalExport_Staging");
        if (Directory.Exists(stagingDir)) Directory.Delete(stagingDir, true);
        Directory.CreateDirectory(stagingDir);

        var context = new ExportContext(scope, stagingDir);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        try
        {
            var projectExporters = selectedExporters.Where(e => e.Order < 50).OrderBy(e => e.Order).ToList();
            foreach (var exp in projectExporters)
            {
                Progress(0.1f, $"Projeto: {exp.ModuleName}...");
                await exp.ExportProject(context);
            }

            var sceneExporters = selectedExporters.Where(e => e.Order >= 50).OrderBy(e => e.Order).ToList();
            float step = 0.6f / Mathf.Max(1, scenesToExport.Count);

            for (int i = 0; i < scenesToExport.Count; i++)
            {
                string path = scenesToExport[i];
                if (string.IsNullOrEmpty(path)) continue;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Progress(0.2f + (i * step), $"Cena: {scene.name}...");
                await RunSceneExporters(sceneExporters, scene, context);
            }
            
            Progress(0.85f, "Resolvendo dependências e assets brutos...");
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

                Debug.Log($"[Universal Exporter] SUCESSO! {scenesToExport.Count} cena(s) e {context.Assets.Copied} assets empacotados.");
                EditorUtility.RevealInFinder(zipPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Universal Exporter] Erro Crítico: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            if (EditorSceneManager.GetActiveScene().path != originalScenePath)
                EditorSceneManager.OpenScene(originalScenePath);
            EditorUtility.ClearProgressBar();
        }
    }

    private static async Task RunSceneExporters(List<IExporter> exporters, Scene scene, ExportContext ctx)
    {
        if (!string.IsNullOrEmpty(scene.path)) ctx.TrackAsset(scene.path);
        
        foreach (var exporter in exporters) await exporter.ExportScene(scene, ctx);
    }

    private static void WriteManifest(ExportContext ctx, string timestamp, ExportScope scope)
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

    private static void GenerateHtmlReport(string stagingDir, string finalHtmlPath)
    {
        string[] guids = AssetDatabase.FindAssets("unity-inspector");
        string htmlContent = "";

        foreach (var guid in guids)
        {
            string path = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(guid));
            if (!File.Exists(path)) continue;
            
            string content = File.ReadAllText(path);
            
            if (!content.Contains("id=\"universal-export-data\"")) 
            {
                htmlContent = content;
                break;
            }
        }

        if (string.IsNullOrEmpty(htmlContent))
        {
            Debug.LogError("[Universal Exporter] Template 'unity-inspector.html' não encontrado!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("<script id=\"universal-export-data\" type=\"application/json\">");
        sb.AppendLine("{ \"files\": [");

        string[] jsonFiles = Directory.GetFiles(stagingDir, "*.json", SearchOption.AllDirectories);
        
        for (int i = 0; i < jsonFiles.Length; i++)
        {
            string fileName = Path.GetFileName(jsonFiles[i]);
            byte[] fileBytes = File.ReadAllBytes(jsonFiles[i]);
            string base64 = Convert.ToBase64String(fileBytes);
            
            string comma = i < jsonFiles.Length - 1 ? "," : "";
            sb.AppendLine($"  {{ \"name\": \"{fileName}\", \"content\": \"{base64}\" }}{comma}");
        }

        sb.AppendLine("] }");
        sb.AppendLine("</script>");

        if (htmlContent.Contains("</head>"))
            htmlContent = htmlContent.Replace("</head>", sb.ToString() + "\n</head>");
        else
            htmlContent += sb.ToString();

        File.WriteAllText(finalHtmlPath, htmlContent, Encoding.UTF8);
    }
}
#endif