#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class LightingExporter : IExporter
{
    public string ModuleName => "lighting";
    public int Order => 50;

    public async Task ExportProject(ExportContext ctx) {await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        var probes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);

        if (lights.Length == 0 && probes.Length == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        
        sb.AppendLine("  \"lights\": [");
        for (int i = 0; i < lights.Length; i++)
        {
            var l = lights[i];
            var comma = i < lights.Length - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(l.gameObject.name)}\", \"hierarchyPath\": \"{ExportUtils.Esc(ExportUtils.GetHierarchyPath(l.transform))}\", \"type\": \"{l.type}\", \"color\": {ExportUtils.ColorJson(l.color)}, \"intensity\": {ExportUtils.F(l.intensity)}, \"range\": {ExportUtils.F(l.range)}, \"spotAngle\": {ExportUtils.F(l.spotAngle)}, \"shadows\": \"{l.shadows}\" }}{comma}");
        }
        sb.AppendLine("  ],");

        sb.AppendLine("  \"reflectionProbes\": [");
        for (int i = 0; i < probes.Length; i++)
        {
            var p = probes[i];
            var comma = i < probes.Length - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(p.gameObject.name)}\", \"hierarchyPath\": \"{ExportUtils.Esc(ExportUtils.GetHierarchyPath(p.transform))}\", \"mode\": \"{p.mode}\", \"intensity\": {ExportUtils.F(p.intensity)}, \"boxSize\": {ExportUtils.V3(p.size)} }}{comma}");
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"lighting_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        await Task.CompletedTask;
    }
}
#endif