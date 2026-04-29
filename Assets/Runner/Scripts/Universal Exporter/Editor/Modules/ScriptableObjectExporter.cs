#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

public class ScriptableObjectExporter : IExporter
{
    public string ModuleName => "scriptable_objects";
    public int Order => 10;

    public async Task ExportScene(UnityEngine.SceneManagement.Scene scene, ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportProject(ExportContext ctx)
    {
        var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });
        int count = 0;

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (so == null || ShouldSkipNamespace(so)) continue;

            if (ProcessScriptableObject(so, ctx.EnsureDir(ModuleName), ctx))
                count++;
        }

        Debug.Log($"[SO Exporter] {count} ScriptableObjects exportados.");
        await Task.CompletedTask;
    }

    bool ShouldSkipNamespace(ScriptableObject so)
    {
        if (so.GetType().Name == "ExportProfile") return true;
        
        var ns = so.GetType().Namespace ?? "";
        return ns.StartsWith("Unity") || ns.StartsWith("TMPro") || ns.StartsWith("Cinemachine");
    }

    bool ProcessScriptableObject(ScriptableObject so, string outDir, ExportContext ctx)
    {
        var jsonBase = EditorJsonUtility.ToJson(so);
        var dict = new Dictionary<string, string>();
        
        var fields = so.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in fields)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType))
            {
                var objRef = f.GetValue(so) as UnityEngine.Object;
                if (objRef != null)
                {
                    var path = AssetDatabase.GetAssetPath(objRef);
                    if (!string.IsNullOrEmpty(path))
                    {
                        ctx.TrackAsset(path);
                        dict[f.Name] = $"{{ \"__assetPath\": \"{ExportUtils.Esc(path)}\", \"__assetType\": \"{objRef.GetType().Name}\" }}";
                    }
                }
            }
        }
        
        var finalJson = InjectDictIntoJson(jsonBase, dict, so.GetType().Name);
        File.WriteAllText(Path.Combine(outDir, so.name + ".json"), finalJson);
        return true;
    }

    string InjectDictIntoJson(string baseJson, Dictionary<string, string> resolvedAssets, string typeName)
    {
        var trimmed = baseJson.Trim();
        if (trimmed.EndsWith("}")) trimmed = trimmed.Substring(0, trimmed.Length - 1);

        var sb = new System.Text.StringBuilder(trimmed);
        
        foreach (var kvp in resolvedAssets)
            sb.Append($",\n  \"{kvp.Key}\": {kvp.Value}");

        sb.Append($",\n  \"_meta\": {{ \"soType\": \"{typeName}\" }}\n}}");
        return sb.ToString();
    }
}
#endif