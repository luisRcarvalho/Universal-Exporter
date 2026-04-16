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
        string destFolder = EditorUtility.SaveFolderPanel("Escolha onde salvar a exportação", "", "");
        
        if (string.IsNullOrEmpty(destFolder))
        {
            Debug.Log("[Universal Exporter] Exportação cancelada pelo usuário.");
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var scopeLabel = scope == ExportScope.CurrentScene ? "scene" : "project";
        
        var stagingDir = Path.Combine(Path.GetTempPath(), $"universal_export_{scopeLabel}_{timestamp}");
        var context = new ExportContext(scope, stagingDir);

        try
        {
            Progress(0.0f, "Iniciando pipeline sólida...");

            if (scope == ExportScope.CurrentScene)
            {
                var scene = SceneManager.GetActiveScene();
                RunProjectExporters(selectedExporters, context);
                RunSceneExporters(selectedExporters, scene, context);
                await ExportGltf(scene, context.EnsureDir("gltf"));
            }
            else
            {
                RunProjectExporters(selectedExporters, context);
                await ExportMultipleScenes(selectedExporters, context);
            }

            Progress(0.85f, $"Copiando assets brutos ({context.Assets.Registered})...");
            context.Assets.CopyAllTo(stagingDir);

            Progress(0.90f, "Gerando manifesto...");
            WriteManifest(context, timestamp, scope);
            
            Progress(0.92f, "Gerando HTML Report...");
            HtmlReportGenerator.Generate(stagingDir, destFolder);
            
            Progress(0.96f, "Comprimindo ZIP...");
            var zipPath = Path.Combine(destFolder, $"universal_export_{scopeLabel}_{timestamp}.zip");
            ZipFile.CreateFromDirectory(stagingDir, zipPath);

            Debug.Log($"[Universal Exporter] ✓ Arquivos salvos com sucesso em: {destFolder}");
            
            EditorUtility.RevealInFinder(zipPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Universal Exporter] Erro: {e}");
            EditorUtility.DisplayDialog("Erro Fatal", e.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            
            if (Directory.Exists(stagingDir)) 
                Directory.Delete(stagingDir, true);
        }
    }

    static void RunProjectExporters(List<IExporter> exporters, ExportContext ctx)
    {
        foreach (var exporter in exporters)
        {
            Progress(0.1f, $"Global: {exporter.ModuleName}...");
            exporter.ExportProject(ctx);
        }
    }

    static void RunSceneExporters(List<IExporter> exporters, Scene scene, ExportContext ctx)
    {
        foreach (var exporter in exporters)
        {
            Progress(0.3f, $"Cena {scene.name}: {exporter.ModuleName}...");
            exporter.ExportScene(scene, ctx);
        }
    }

    static async Task ExportMultipleScenes(List<IExporter> exporters, ExportContext ctx)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();
        if (scenes.Count == 0) scenes.Add(SceneManager.GetActiveScene().path);

        foreach (var path in scenes)
        {
            if (SceneManager.GetActiveScene().path != path)
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var scene = SceneManager.GetActiveScene();
            
            var sceneSubDir = Path.Combine(ctx.StagingDir, "scenes", scene.name);
            var subContext = new ExportContext(ctx.Scope, sceneSubDir);

            RunSceneExporters(exporters, scene, subContext);
            await ExportGltf(scene, subContext.EnsureDir("gltf"));
        }
    }

    static async Task ExportGltf(Scene scene, string outDir)
    {
#if GLTFAST_INSTALLED
        var settings = new ExportSettings { Format = GltfFormat.Binary, FileConflictResolution = FileConflictResolution.Overwrite };
        var exporter = new GameObjectExport(settings);
        exporter.AddScene(scene.GetRootGameObjects(), scene.name);
        await exporter.SaveToFileAndDispose(Path.Combine(outDir, $"{scene.name}.glb"));
#else
        Debug.LogWarning("[Universal Exporter] glTFast não instalado. Pulo do GLB.");
        await Task.CompletedTask;
#endif
    }

    static void WriteManifest(ExportContext ctx, string timestamp, ExportScope scope)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"exportVersion\": \"{VERSION}\",");
        sb.AppendLine($"  \"scope\": \"{scope}\"");
        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(ctx.StagingDir, "export_manifest.json"), sb.ToString(), Encoding.UTF8);
    }

    static void Progress(float t, string msg) => EditorUtility.DisplayProgressBar("Universal Exporter", msg, t);
}
#endif