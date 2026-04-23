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

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

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
            
            var pos = cam.transform.position;
            var rot = cam.transform.eulerAngles;

            sb.AppendLine("    {");
            sb.AppendLine($"      \"name\": \"{ExportUtils.Esc(cam.name)}\",");
            sb.AppendLine($"      \"position\": {{ \"x\": {ExportUtils.F(pos.x)}, \"y\": {ExportUtils.F(pos.y)}, \"z\": {ExportUtils.F(pos.z)} }},");
            sb.AppendLine($"      \"rotation\": {{ \"x\": {ExportUtils.F(rot.x)}, \"y\": {ExportUtils.F(rot.y)}, \"z\": {ExportUtils.F(rot.z)} }},");
            sb.AppendLine($"      \"isOrthographic\": {ExportUtils.B(cam.orthographic)},");
            sb.AppendLine($"      \"orthographicSize\": {ExportUtils.F(cam.orthographicSize)},");
            sb.AppendLine($"      \"fieldOfView\": {ExportUtils.F(cam.fieldOfView)},");
            sb.AppendLine($"      \"nearClipPlane\": {ExportUtils.F(cam.nearClipPlane)},");
            sb.AppendLine($"      \"farClipPlane\": {ExportUtils.F(cam.farClipPlane)},");
            sb.AppendLine($"      \"depth\": {ExportUtils.F(cam.depth)},");
            sb.AppendLine($"      \"clearFlags\": \"{cam.clearFlags}\"");
            sb.AppendLine($"    }}{comma}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"camera_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        await Task.CompletedTask;
    }
}
#endif