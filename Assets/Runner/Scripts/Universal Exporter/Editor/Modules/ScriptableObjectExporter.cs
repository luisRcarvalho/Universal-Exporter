#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectExporter : IExporter
{
    public string ModuleName => "data";
    public int Order => 10; // Executa logo no começo

    public void ExportScene(UnityEngine.SceneManagement.Scene scene, ExportContext ctx) 
    { 
        // SOs são do projeto, não da cena. Deixamos vazio.
    }

    public void ExportProject(ExportContext ctx)
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
    }

    bool ShouldSkipNamespace(ScriptableObject so)
    {
        var ns = so.GetType().Namespace ?? "";
        return ns.StartsWith("Unity") || ns.StartsWith("TMPro") || ns.StartsWith("Cinemachine");
    }

    bool ProcessScriptableObject(ScriptableObject so, string outDir, ExportContext ctx)
    {
        var jsonBase = EditorJsonUtility.ToJson(so);
        var dict = new Dictionary<string, string>();

        // Usamos reflection para pegar assets (áudios, prefabs, etc.) e registrar no AssetTracker
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

        // Limpeza simples do JSON Base e injeção do dicionário
        var finalJson = InjectDictIntoJson(jsonBase, dict, so.GetType().Name);
        File.WriteAllText(Path.Combine(outDir, so.name + ".json"), finalJson);
        return true;
    }

    string InjectDictIntoJson(string baseJson, Dictionary<string, string> resolvedAssets, string typeName)
    {
        // Remove a última chave '}'
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