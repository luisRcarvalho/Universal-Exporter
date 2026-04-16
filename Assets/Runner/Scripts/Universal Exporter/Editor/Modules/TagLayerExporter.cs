#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditorInternal;
using UnityEngine;

public class TagLayerExporter : IExporter
{
    public string ModuleName => "tags_layers";
    public int Order => 20;

    public void ExportScene(UnityEngine.SceneManagement.Scene scene, ExportContext ctx) { }

    public void ExportProject(ExportContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"tags\": [\"{string.Join("\", \"", InternalEditorUtility.tags)}\"],");
        sb.AppendLine($"  \"layers\": [\"{string.Join("\", \"", InternalEditorUtility.layers)}\"]");
        sb.AppendLine("}");
        
        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), "tags_layers_global.json"), sb.ToString());
        Debug.Log("[TagLayerExporter] Dicionário de Tags e Layers exportado.");
    }
}
#endif