#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class PhysicsExporter : IExporter
{
    public string ModuleName => "physics";
    public int Order => 30;

    public async Task ExportProject(ExportContext ctx) {await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        
        sb.AppendLine("  \"global3D\": {");
        sb.AppendLine($"    \"gravity\": {ExportUtils.V3(Physics.gravity)},");
        sb.AppendLine($"    \"defaultContactOffset\": {ExportUtils.F(Physics.defaultContactOffset)},");
        sb.AppendLine($"    \"sleepThreshold\": {ExportUtils.F(Physics.sleepThreshold)}");
        sb.AppendLine("  },");
        
        var rbs = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        sb.AppendLine("  \"rigidbodies3D\": [");
        for (int i = 0; i < rbs.Length; i++)
        {
            var rb = rbs[i];
            var comma = i < rbs.Length - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(rb.gameObject.name)}\", \"mass\": {ExportUtils.F(rb.mass)}, \"isKinematic\": {ExportUtils.B(rb.isKinematic)}, \"useGravity\": {ExportUtils.B(rb.useGravity)} }}{comma}");
        }
        sb.AppendLine("  ],");
        
        var colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
        sb.AppendLine("  \"colliders3D\": [");
        for (int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];
            var comma = i < colliders.Length - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(col.gameObject.name)}\", \"type\": \"{col.GetType().Name}\", \"isTrigger\": {ExportUtils.B(col.isTrigger)} }}{comma}");
        }
        sb.AppendLine("  ]");
        
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"physics_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        await Task.CompletedTask;
    }
}
#endif