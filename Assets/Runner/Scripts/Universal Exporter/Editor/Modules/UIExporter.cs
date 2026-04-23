#if UNITY_EDITOR
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiExporter : IExporter
{
    public string ModuleName => "ui";
    public int Order => 80;

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"canvases\": [");

        for (int i = 0; i < canvases.Length; i++)
        {
            var canvas = canvases[i];
            var comma = i < canvases.Length - 1 ? "," : "";
            
            sb.AppendLine("    {");
            sb.AppendLine($"      \"name\": \"{ExportUtils.Esc(canvas.name)}\",");
            sb.AppendLine($"      \"renderMode\": \"{canvas.renderMode}\",");
            sb.AppendLine($"      \"sortingOrder\": {canvas.sortingOrder},");
            
            var rt = canvas.GetComponent<RectTransform>();
            sb.AppendLine($"      \"size\": {{ \"w\": {ExportUtils.F(rt.rect.width)}, \"h\": {ExportUtils.F(rt.rect.height)} }},");
            
            sb.AppendLine("      \"elements\": [");
            ExportUiRecursive(canvas.transform, sb);
            sb.AppendLine("      ]");
            sb.AppendLine($"    }}{comma}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"ui_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        await Task.CompletedTask;
    }

    private void ExportUiRecursive(Transform parent, StringBuilder sb)
    {
        var children = new List<RectTransform>();
        foreach (Transform child in parent)
        {
            var rt = child.GetComponent<RectTransform>();
            if (rt != null && child.gameObject.activeSelf) children.Add(rt);
        }

        for (int i = 0; i < children.Count; i++)
        {
            var rt = children[i];
            var comma = i < children.Count - 1 ? "," : "";
            
            sb.AppendLine("        {");
            sb.AppendLine($"          \"name\": \"{ExportUtils.Esc(rt.name)}\",");
            sb.AppendLine($"          \"pos\": {{ \"x\": {ExportUtils.F(rt.localPosition.x)}, \"y\": {ExportUtils.F(rt.localPosition.y)} }},");
            sb.AppendLine($"          \"size\": {{ \"w\": {ExportUtils.F(rt.rect.width)}, \"h\": {ExportUtils.F(rt.rect.height)} }},");
            sb.AppendLine($"          \"pivot\": {{ \"x\": {ExportUtils.F(rt.pivot.x)}, \"y\": {ExportUtils.F(rt.pivot.y)} }},");
            sb.AppendLine("          \"children\": [");
            ExportUiRecursive(rt, sb);
            sb.AppendLine("          ]");
            sb.AppendLine($"        }}{comma}");
        }
    }
}
#endif