#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

public class ProjectSettingsExporter : IExporter
{
    public string ModuleName => "project_settings";
    public int Order => 10;

    public void ExportScene(UnityEngine.SceneManagement.Scene scene, ExportContext ctx) { }

    public void ExportProject(ExportContext ctx)
    {
        var outDir = ctx.EnsureDir(ModuleName);
        
        var timeJson = $"{{ \"fixedDeltaTime\": {ExportUtils.F(Time.fixedDeltaTime)}, \"timeScale\": {ExportUtils.F(Time.timeScale)} }}";
        File.WriteAllText(Path.Combine(outDir, "time_settings.json"), timeJson);

        var qualityJson = $"{{ \"activeLevelName\": \"{QualitySettings.names[QualitySettings.GetQualityLevel()]}\", \"vSyncCount\": {QualitySettings.vSyncCount} }}";
        File.WriteAllText(Path.Combine(outDir, "quality_settings.json"), qualityJson);
        
        var playerJson = $"{{ \"companyName\": \"{ExportUtils.Esc(PlayerSettings.companyName)}\", \"productName\": \"{ExportUtils.Esc(PlayerSettings.productName)}\", \"bundleVersion\": \"{ExportUtils.Esc(PlayerSettings.bundleVersion)}\", \"unityVersion\": \"{Application.unityVersion}\" }}";
        File.WriteAllText(Path.Combine(outDir, "player_settings.json"), playerJson);

#if UNITY_URP
        var urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urp != null)
        {
            var urpJson = $"{{ \"renderScale\": {ExportUtils.F(urp.renderScale)}, \"hdr\": {ExportUtils.B(urp.supportsHDR)}, \"shadowDistance\": {ExportUtils.F(urp.shadowDistance)} }}";
            File.WriteAllText(Path.Combine(outDir, "urp_settings.json"), urpJson);
        }
#endif
    }
}
#endif