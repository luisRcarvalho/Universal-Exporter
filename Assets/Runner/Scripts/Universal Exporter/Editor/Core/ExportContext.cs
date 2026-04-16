#if UNITY_EDITOR
using System.IO;

public enum ExportScope
{
    CurrentScene,
    FullProject,
}

public sealed class ExportContext
{
    public ExportScope Scope { get; }
    public string StagingDir { get; }
    public AssetTracker Assets { get; }

    public ExportContext(ExportScope scope, string stagingDir)
    {
        Scope = scope;
        StagingDir = stagingDir;
        Assets = new AssetTracker();
        Directory.CreateDirectory(stagingDir);
    }

    public string TrackAsset(string assetPath) => Assets.Register(assetPath);
    
    public string EnsureDir(string moduleName)
    {
        var path = Path.Combine(StagingDir, moduleName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }
}
#endif