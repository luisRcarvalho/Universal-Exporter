#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class AudioClipMetadataExporter : IExporter
{
    public string ModuleName => "audio";
    public int Order => 70;

    public async Task ExportProject(ExportContext ctx) {await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        if (sources.Length == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"audioSources\": [");

        for (int i = 0; i < sources.Length; i++)
        {
            var s = sources[i];
            var comma = i < sources.Length - 1 ? "," : "";
            
            string clipPath = "";
            if (s.clip != null)
            {
                clipPath = AssetDatabase.GetAssetPath(s.clip);
                ctx.TrackAsset(clipPath); // Registar ficheiro para ir no ZIP
            }

            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(s.gameObject.name)}\", \"hierarchyPath\": \"{ExportUtils.Esc(ExportUtils.GetHierarchyPath(s.transform))}\", \"volume\": {ExportUtils.F(s.volume)}, \"pitch\": {ExportUtils.F(s.pitch)}, \"loop\": {ExportUtils.B(s.loop)}, \"playOnAwake\": {ExportUtils.B(s.playOnAwake)}, \"spatialBlend\": {ExportUtils.F(s.spatialBlend)}, \"minDistance\": {ExportUtils.F(s.minDistance)}, \"maxDistance\": {ExportUtils.F(s.maxDistance)}, \"clipPath\": \"{ExportUtils.Esc(clipPath)}\" }}{comma}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"audio_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        Debug.Log($"[AudioExporter] {sources.Length} AudioSources exportados.");
        await Task.CompletedTask;
    }
}
#endif