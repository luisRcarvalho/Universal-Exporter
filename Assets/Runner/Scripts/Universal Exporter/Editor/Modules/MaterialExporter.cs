#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MaterialExporter : IExporter
{
    public string ModuleName => "materials";
    public int Order => 52; // CORRIGIDO!

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var materialsMap = new Dictionary<string, Material>();

        foreach (var r in renderers)
        {
            if (r == null || r.sharedMaterials == null) continue;
            foreach (var mat in r.sharedMaterials)
                if (mat != null && !materialsMap.ContainsKey(mat.name))
                    materialsMap[mat.name] = mat;
        }

        if (materialsMap.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"materials\": [");

        var matList = new List<Material>(materialsMap.Values);
        for (int i = 0; i < matList.Count; i++)
        {
            var mat = matList[i];
            var comma = i < matList.Count - 1 ? "," : "";
            
            var color = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : (mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white);
            
            var metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
            var smoothness = mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : (mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0f);

            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(mat.name)}\", \"shader\": \"{ExportUtils.Esc(mat.shader.name)}\", \"color\": {ExportUtils.ColorJson(color)}, \"metallic\": {ExportUtils.F(metallic)}, \"smoothness\": {ExportUtils.F(smoothness)} }}{comma}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"materials_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        await Task.CompletedTask;
    }
}
#endif