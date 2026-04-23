#if UNITY_EDITOR
using System.IO;

public enum ExportScope { CurrentScene, FullProject }

public class ExportContext
{
    public ExportScope Scope { get; }
    public string StagingDir { get; }
    public AssetTracker Assets { get; } = new AssetTracker();

    public ExportContext(ExportScope scope, string stagingDir)
    {
        Scope = scope;
        StagingDir = stagingDir;
    }

    public string EnsureDir(string moduleName)
    {
        var path = Path.Combine(StagingDir, "data", moduleName);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }
    public void TrackAsset(string path) => Assets.TrackAsset(path);
}
#endif