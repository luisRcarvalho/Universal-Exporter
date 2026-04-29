#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneHierarchyExporter : IExporter
{
    public string ModuleName => "hierarchy";
    public int Order => 90;

    public async Task ExportProject(ExportContext ctx) {await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var roots = scene.GetRootGameObjects();
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"gameObjects\": [");

        for (int i = 0; i < roots.Length; i++)
        {
            var comma = i < roots.Length - 1 ? "," : "";
            sb.Append(SerializeGameObject(roots[i], 4, ctx));
            sb.AppendLine(comma);
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"hierarchy_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        Debug.Log($"[HierarchyExporter] {roots.Length} raízes da cena exportadas.");
        await Task.CompletedTask;
    }

    string SerializeGameObject(GameObject go, int indent, ExportContext ctx)
    {
        var pad = ExportUtils.Pad(indent);
        var pad2 = ExportUtils.Pad(indent + 2);
        var sb = new StringBuilder();

        sb.AppendLine($"{pad}{{");
        sb.AppendLine($"{pad2}\"name\": \"{ExportUtils.Esc(go.name)}\",");
        sb.AppendLine($"{pad2}\"active\": {ExportUtils.B(go.activeSelf)},");
        sb.AppendLine($"{pad2}\"tag\": \"{ExportUtils.Esc(go.tag)}\",");
        sb.AppendLine($"{pad2}\"layer\": \"{ExportUtils.Esc(LayerMask.LayerToName(go.layer))}\",");
        
        var pos = go.transform.localPosition;
        var rot = go.transform.localEulerAngles;
        var scl = go.transform.localScale;
        
        sb.AppendLine($"{pad2}\"transform\": {{ \"position\": {ExportUtils.V3(pos)}, \"rotation\": {ExportUtils.V3(rot)}, \"scale\": {ExportUtils.V3(scl)} }},");

        var comps = new List<string>();
        foreach (var c in go.GetComponents<Component>())
        {
            if (c == null || c.GetType().Name == "Transform") continue;
            comps.Add($"\"{c.GetType().Name}\": {SerializeComponent(c, ctx)}");
        }
        
        sb.AppendLine($"{pad2}\"components\": {{ {string.Join(", ", comps)} }},");
        sb.AppendLine($"{pad2}\"children\": [");

        for (int i = 0; i < go.transform.childCount; i++)
        {
            var child = go.transform.GetChild(i).gameObject;
            var comma = i < go.transform.childCount - 1 ? "," : "";
            sb.Append(SerializeGameObject(child, indent + 4, ctx));
            sb.AppendLine(comma);
        }

        sb.AppendLine($"{pad2}]");
        sb.Append($"{pad}}}");
        return sb.ToString();
    }

    string SerializeComponent(Component comp, ExportContext ctx)
    {
        var fields = new List<string>();
        var type = comp.GetType();
        foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
            {
                var val = f.GetValue(comp);
                fields.Add($"\"{ExportUtils.Esc(f.Name)}\": {SerializeValue(val, ctx)}");
            }
        }
        return $"{{ {string.Join(", ", fields)} }}";
    }

    string SerializeValue(object val, ExportContext ctx)
    {
        if (val == null) return "null";
        if (val is int || val is float || val is double) return val.ToString().Replace(",", ".");
        if (val is bool b) return ExportUtils.B(b);
        if (val is string s) return $"\"{ExportUtils.Esc(s)}\"";
        if (val is Vector2 v2) return ExportUtils.V2(v2);
        if (val is Vector3 v3) return ExportUtils.V3(v3);
        if (val is Color c) return ExportUtils.ColorJson(c);
        if (val is UnityEngine.Object obj && obj != null)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path)) ctx.TrackAsset(path);
            return $"{{ \"__assetPath\": \"{ExportUtils.Esc(path)}\" }}";
        }
        return $"\"{ExportUtils.Esc(val.ToString())}\"";
    }
}
#endif