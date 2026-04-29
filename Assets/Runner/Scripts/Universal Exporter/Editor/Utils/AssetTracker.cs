#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class AssetTracker
{
    private HashSet<string> _registeredAssets = new HashSet<string>();

    public int Registered => _registeredAssets.Count;
    public int Copied { get; private set; }
    public int Skipped { get; private set; }

    public void TrackAsset(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (path.EndsWith(".cs") || path.StartsWith("Packages/") || path.Contains("unity_builtin_extra")) return;
        
        _registeredAssets.Add(path);
    }

    // A BALA DE PRATA: Puxa texturas, materiais, fbx e animações baseados no que já foi encontrado!
    public void ResolveDependencies()
    {
        // Pega tudo que já registramos (A cena, os audios, os scriptable objects)
        string[] currentPaths = _registeredAssets.ToArray();
        
        // Pede para a Unity puxar a árvore completa de dependências (o "true" significa busca profunda)
        string[] allDependencies = AssetDatabase.GetDependencies(currentPaths, true);

        foreach (var dep in allDependencies)
        {
            if (dep.EndsWith(".cs") || dep.StartsWith("Packages/") || dep.Contains("unity_builtin_extra")) continue;
            _registeredAssets.Add(dep);
        }
    }

    public void CopyAllTo(string stagingDir)
    {
        Copied = 0;
        Skipped = 0;

        foreach (var assetPath in _registeredAssets)
        {
            var fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                Skipped++;
                continue;
            }

            var destPath = Path.Combine(stagingDir, "assets", assetPath);
            var destDir = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            File.Copy(fullPath, destPath, true);
            Copied++;
        }
    }
}
#endif