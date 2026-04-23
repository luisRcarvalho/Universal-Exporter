#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

public class ProjectSettingsExporter : IExporter
{
    public string ModuleName => "project_settings";
    public int Order => 10;

    public async Task ExportScene(UnityEngine.SceneManagement.Scene scene, ExportContext ctx) { await Task.CompletedTask;}

    public async Task ExportProject(ExportContext ctx)
    {
        var outDir = ctx.EnsureDir(ModuleName);
        
        var timeJson = $"{{ \"fixedDeltaTime\": {ExportUtils.F(Time.fixedDeltaTime)}, \"maximumDeltaTime\": {ExportUtils.F(Time.maximumDeltaTime)}, \"timeScale\": {ExportUtils.F(Time.timeScale)} }}";
        File.WriteAllText(Path.Combine(outDir, "time_settings.json"), timeJson);
        
        var qualityJson = $"{{ \"activeLevelName\": \"{QualitySettings.names[QualitySettings.GetQualityLevel()]}\", \"vSyncCount\": {QualitySettings.vSyncCount}, \"shadows\": \"{QualitySettings.shadows}\", \"shadowResolution\": \"{QualitySettings.shadowResolution}\", \"shadowDistance\": {ExportUtils.F(QualitySettings.shadowDistance)}, \"antiAliasing\": {QualitySettings.antiAliasing}, \"anisotropicFiltering\": \"{QualitySettings.anisotropicFiltering}\" }}";
        File.WriteAllText(Path.Combine(outDir, "quality_settings.json"), qualityJson);
        
        var playerJson = $"{{ \"companyName\": \"{ExportUtils.Esc(PlayerSettings.companyName)}\", \"productName\": \"{ExportUtils.Esc(PlayerSettings.productName)}\", \"bundleVersion\": \"{ExportUtils.Esc(PlayerSettings.bundleVersion)}\", \"unityVersion\": \"{Application.unityVersion}\", \"defaultScreenWidth\": {PlayerSettings.defaultScreenWidth}, \"defaultScreenHeight\": {PlayerSettings.defaultScreenHeight} }}";
        File.WriteAllText(Path.Combine(outDir, "player_settings.json"), playerJson);
        
        var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        if (pipeline != null && pipeline.GetType().Name.Contains("UniversalRenderPipelineAsset"))
        {
            var type = pipeline.GetType();
            var renderScale = type.GetProperty("renderScale")?.GetValue(pipeline) ?? 1f;
            var hdr = type.GetProperty("supportsHDR")?.GetValue(pipeline) ?? false;
            var msaa = type.GetProperty("msaaSampleCount")?.GetValue(pipeline) ?? 1;
            var shadowDist = type.GetProperty("shadowDistance")?.GetValue(pipeline) ?? 0f;
            var cascades = type.GetProperty("shadowCascadeCount")?.GetValue(pipeline) ?? 1;
            var softShadows = type.GetProperty("supportsSoftShadows")?.GetValue(pipeline) ?? false;

            var urpJson = $"{{ \"renderScale\": {ExportUtils.F((float)renderScale)}, \"hdr\": {ExportUtils.B((bool)hdr)}, \"msaaSampleCount\": {msaa}, \"shadowDistance\": {ExportUtils.F((float)shadowDist)}, \"shadowCascadeCount\": {cascades}, \"supportsSoftShadows\": {ExportUtils.B((bool)softShadows)} }}";
            File.WriteAllText(Path.Combine(outDir, "urp_settings.json"), urpJson);
        }
        await Task.CompletedTask;
    }
}
#endif