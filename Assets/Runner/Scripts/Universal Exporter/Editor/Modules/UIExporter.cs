#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class UIExporter : IExporter
{
    public string ModuleName => "ui";
    public int Order => 80;

    public async Task ExportProject(ExportContext ctx) {await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var roots = new List<Canvas>();
        foreach (var c in canvases) if (c.isRootCanvas) roots.Add(c);

        if (roots.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"canvases\": [");

        for (int i = 0; i < roots.Count; i++)
        {
            var comma = i < roots.Count - 1 ? "," : "";
            sb.Append(SerializeElement(roots[i].gameObject, 4, ctx));
            sb.AppendLine(comma);
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"ui_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        Debug.Log($"[UIExporter] {roots.Count} Canvas raízes exportados.");
        await Task.CompletedTask;
    }

    string SerializeElement(GameObject go, int indent, ExportContext ctx)
    {
        var pad = ExportUtils.Pad(indent);
        var pad2 = ExportUtils.Pad(indent + 2);
        var sb = new StringBuilder();

        sb.AppendLine($"{pad}{{");
        sb.AppendLine($"{pad2}\"name\": \"{ExportUtils.Esc(go.name)}\",");
        sb.AppendLine($"{pad2}\"active\": {ExportUtils.B(go.activeSelf)},");

        // Componentes...
        var parts = new List<string>();

        var rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            parts.Add($"{pad2}\"RectTransform\": {{\n" +
                      $"{pad2}  \"anchoredPosition\": {ExportUtils.V2(rect.anchoredPosition)},\n" +
                      $"{pad2}  \"sizeDelta\": {ExportUtils.V2(rect.sizeDelta)},\n" +
                      $"{pad2}  \"anchorMin\": {ExportUtils.V2(rect.anchorMin)},\n" +
                      $"{pad2}  \"anchorMax\": {ExportUtils.V2(rect.anchorMax)},\n" +
                      $"{pad2}  \"pivot\": {ExportUtils.V2(rect.pivot)}\n" +
                      $"{pad2}}}");
        }

        var img = go.GetComponent<Image>();
        if (img != null)
        {
            string spritePath = img.sprite ? ExportUtils.Esc(UnityEditor.AssetDatabase.GetAssetPath(img.sprite)) : "";
            if (!string.IsNullOrEmpty(spritePath)) ctx.TrackAsset(spritePath);
            parts.Add($"{pad2}\"Image\": {{ \"color\": {ExportUtils.ColorJson(img.color)}, \"spritePath\": \"{spritePath}\" }}");
        }
        
        for (int i = 0; i < parts.Count; i++)
        {
            sb.AppendLine(parts[i] + (i < parts.Count - 1 || go.transform.childCount > 0 ? "," : ""));
        }
        
        if (go.transform.childCount > 0)
        {
            sb.AppendLine($"{pad2}\"children\": [");
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                sb.Append(SerializeElement(child, indent + 4, ctx));
                if (i < go.transform.childCount - 1) sb.AppendLine(",");
                else sb.AppendLine();
            }
            sb.AppendLine($"{pad2}]");
        }

        sb.Append($"{pad}}}");
        return sb.ToString();
    }
}
#endif