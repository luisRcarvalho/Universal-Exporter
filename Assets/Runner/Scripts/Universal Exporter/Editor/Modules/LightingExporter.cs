#if UNITY_EDITOR
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightingExporter : IExporter
{
    public string ModuleName => "lighting";
    public int Order => 55;

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        if (lights.Length == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"lights\": [");

        for (int i = 0; i < lights.Length; i++)
        {
            var lt = lights[i];
            var comma = i < lights.Length - 1 ? "," : "";
            
            var color = ExportUtils.ColorJson(lt.color);
            var rot = lt.transform.eulerAngles;
            
            sb.AppendLine("    {");
            sb.AppendLine($"      \"name\": \"{ExportUtils.Esc(lt.name)}\",");
            sb.AppendLine($"      \"type\": \"{lt.type}\",");
            sb.AppendLine($"      \"intensity\": {ExportUtils.F(lt.intensity)},");
            sb.AppendLine($"      \"color\": {color},");
            sb.AppendLine($"      \"shadows\": \"{lt.shadows}\",");
            sb.AppendLine($"      \"range\": {ExportUtils.F(lt.range)},");
            
            sb.AppendLine($"      \"rotation\": {{ \"x\": {ExportUtils.F(rot.x)}, \"y\": {ExportUtils.F(rot.y)}, \"z\": {ExportUtils.F(rot.z)} }}");
            sb.AppendLine($"    }}{comma}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"lighting_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        await Task.CompletedTask;
    }
}
#endif