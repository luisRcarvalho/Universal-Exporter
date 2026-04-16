#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysicsExporter : IExporter
{
    public string ModuleName => "physics";
    public int Order => 60;

    public void ExportProject(ExportContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        
        sb.AppendLine("  \"global3D\": {");
        sb.AppendLine($"    \"gravity\": {ExportUtils.V3(Physics.gravity)},");
        sb.AppendLine($"    \"defaultContactOffset\": {ExportUtils.F(Physics.defaultContactOffset)},");
        sb.AppendLine($"    \"sleepThreshold\": {ExportUtils.F(Physics.sleepThreshold)},");
        sb.AppendLine($"    \"defaultSolverIterations\": {Physics.defaultSolverIterations}");
        sb.AppendLine("  },");
        
        sb.AppendLine("  \"collisionMatrix3D\": [");
        for (int i = 0; i < 32; i++)
        {
            var row = new List<string>();
            for (int j = 0; j < 32; j++)
                row.Add(ExportUtils.B(!Physics.GetIgnoreLayerCollision(i, j)));
            
            var comma = i < 31 ? "," : "";
            sb.AppendLine($"    [{string.Join(", ", row)}]{comma}");
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), "physics_settings.json"), sb.ToString(), Encoding.UTF8);
    }

    public void ExportScene(Scene scene, ExportContext ctx)
    {
        var rigidbodies = UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        var colliders = UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
        if (rigidbodies.Length == 0 && colliders.Length == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"rigidbodies3D\": ["); // Nome corrigido
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            var rb = rigidbodies[i];
            var comma = i < rigidbodies.Length - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(rb.gameObject.name)}\", \"mass\": {ExportUtils.F(rb.mass)} }}{comma}");
        }
        sb.AppendLine("  ],");
        sb.AppendLine("  \"colliders3D\": ["); // Nome corrigido
        for (int i = 0; i < colliders.Length; i++)
        {
            var c = colliders[i];
            var comma = i < colliders.Length - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(c.gameObject.name)}\", \"type\": \"{c.GetType().Name}\" }}{comma}");
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"physics_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
    }
}
#endif