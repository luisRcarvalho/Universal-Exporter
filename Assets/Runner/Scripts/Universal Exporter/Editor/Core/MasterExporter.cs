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
    private static void Progress(float p, string msg) => EditorUtility.DisplayProgressBar("Universal Exporter", msg, p);

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
            foreach (var exp in projectExporters) await exp.ExportProject(context);

            var sceneExporters = selectedExporters.Where(e => e.Order >= 50).OrderBy(e => e.Order).ToList();
            float step = 0.6f / Mathf.Max(1, scenesToExport.Count);

            for (int i = 0; i < scenesToExport.Count; i++)
            {
                string path = scenesToExport[i];
                if (string.IsNullOrEmpty(path)) continue;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Progress(0.2f + (i * step), $"Processando Cena: {scene.name}...");
                await RunSceneExporters(sceneExporters, scene, context);
            }

            context.Assets.ResolveDependencies();
            context.Assets.CopyAllTo(stagingDir);
            
            WriteManifest(context, timestamp, scope, scenesToExport);

            string zipPath = EditorUtility.SaveFilePanel("Salvar Exportação", "", $"Export_{timestamp}", "zip");
            if (!string.IsNullOrEmpty(zipPath))
            {
                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(stagingDir, zipPath);
                GenerateHtmlReport(stagingDir, zipPath.Replace(".zip", ".html"));
                EditorUtility.RevealInFinder(zipPath);
            }
        }
        catch (Exception e) { Debug.LogError($"[MasterExporter] {e.Message}"); }
        finally
        {
            if (EditorSceneManager.GetActiveScene().path != originalScenePath)
                EditorSceneManager.OpenScene(originalScenePath);
            EditorUtility.ClearProgressBar();
        }
    }

    private static void WriteManifest(ExportContext ctx, string timestamp, ExportScope scope, List<string> scenes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"projectName\": \"{ExportUtils.Esc(Application.productName)}\",");
        sb.AppendLine($"  \"exportedAt\": \"{DateTime.Now:dd/MM/yyyy HH:mm}\",");
        sb.AppendLine($"  \"unityVersion\": \"{Application.unityVersion}\",");
        sb.AppendLine("  \"scenes\": [");
        for (int i = 0; i < scenes.Count; i++)
        {
            string sName = Path.GetFileNameWithoutExtension(scenes[i]);
            sb.AppendLine($"    \"{ExportUtils.Esc(sName)}\"{(i < scenes.Count - 1 ? "," : "")}");
        }
        sb.AppendLine("  ],");
        sb.AppendLine($"  \"rawAssets\": {{ \"registered\": {ctx.Assets.Registered}, \"copied\": {ctx.Assets.Copied} }}");
        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(ctx.StagingDir, "export_manifest.json"), sb.ToString());
    }

    private static async Task RunSceneExporters(List<IExporter> exporters, Scene scene, ExportContext ctx)
    {
        foreach (var exporter in exporters) await exporter.ExportScene(scene, ctx);
    }

    private static void GenerateHtmlReport(string stagingDir, string finalHtmlPath)
    {
        string[] guids = AssetDatabase.FindAssets("unity-inspector");
        string htmlTemplate = "";
        foreach (var guid in guids) {
            string p = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(guid));
            if (File.Exists(p) && !File.ReadAllText(p).Contains("id=\"universal-export-data\"")) {
                htmlTemplate = File.ReadAllText(p); break;
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<script id=\"universal-export-data\" type=\"application/json\">");
        sb.AppendLine("{ \"files\": [");
        string[] files = Directory.GetFiles(stagingDir, "*.json", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++) {
            string base64 = Convert.ToBase64String(File.ReadAllBytes(files[i]));
            sb.AppendLine($"  {{ \"name\": \"{Path.GetFileName(files[i])}\", \"content\": \"{base64}\" }}{(i < files.Length - 1 ? "," : "")}");
        }
        sb.AppendLine("] }</script>");
        File.WriteAllText(finalHtmlPath, htmlTemplate.Replace("</head>", sb.ToString() + "</head>"), Encoding.UTF8);
    }
}
#endif