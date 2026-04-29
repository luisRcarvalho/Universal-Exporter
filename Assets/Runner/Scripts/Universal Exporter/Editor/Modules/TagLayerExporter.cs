#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditorInternal;
using UnityEngine;
using System.Threading.Tasks;

public class TagLayerExporter : IExporter
{
    public string ModuleName => "tags_layers";
    public int Order => 20;

    public async Task ExportScene(UnityEngine.SceneManagement.Scene scene, ExportContext ctx) {await Task.CompletedTask; }

    public async Task ExportProject(ExportContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"tags\": [\"{string.Join("\", \"", InternalEditorUtility.tags)}\"],");
        sb.AppendLine($"  \"layers\": [\"{string.Join("\", \"", InternalEditorUtility.layers)}\"]");
        sb.AppendLine("}");
        
        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), "tags_layers_global.json"), sb.ToString());
        Debug.Log("[TagLayerExporter] Dicionário de Tags e Layers exportado.");
        await Task.CompletedTask;
    }
}
#endif