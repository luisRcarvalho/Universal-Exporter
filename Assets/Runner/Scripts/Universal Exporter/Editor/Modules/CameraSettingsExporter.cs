#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class CameraSettingsExporter : IExporter
{
    public string ModuleName => "camera";
    public int Order => 65;

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask;}

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        if (cameras.Length == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"cameras\": [");

        for (int i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            var comma = i < cameras.Length - 1 ? "," : "";
            
            sb.AppendLine($"    {{");
            sb.AppendLine($"      \"name\": \"{ExportUtils.Esc(cam.gameObject.name)}\",");
            sb.AppendLine($"      \"orthographic\": {ExportUtils.B(cam.orthographic)},");
            sb.AppendLine($"      \"fieldOfView\": {ExportUtils.F(cam.fieldOfView)},");
            sb.AppendLine($"      \"nearClipPlane\": {ExportUtils.F(cam.nearClipPlane)},");
            sb.AppendLine($"      \"farClipPlane\": {ExportUtils.F(cam.farClipPlane)},");
            sb.AppendLine($"      \"backgroundColor\": {ExportUtils.ColorJson(cam.backgroundColor)}");
            sb.AppendLine($"    }}{comma}");
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"camera_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        Debug.Log($"[CameraExporter] {cameras.Length} câmera(s) exportada(s).");
        await Task.CompletedTask;
    }
}
#endif