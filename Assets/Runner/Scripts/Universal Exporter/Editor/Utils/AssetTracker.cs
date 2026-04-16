// AssetTracker.cs  v4
// Mudancas em relacao a v3:
//   - Adicionado .anim em IncludedExtensions
//   - Adicionado caso .anim -> assets/animations em BuildDestPath

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetTracker
{
    static readonly HashSet<string> IncludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Audio
        ".wav", ".ogg", ".mp3", ".aiff",
        // Texturas e sprites 2D
        ".png", ".jpg", ".jpeg", ".tga", ".exr", ".hdr", ".psd",
        // Modelos 3D brutos
        ".fbx", ".obj",
        // Clips de animacao standalone
        ".anim",
        // Materiais Unity
        ".mat",
        // Fontes
        ".ttf", ".otf",
        // Dados
        ".json", ".xml", ".csv",
    };

    static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".meta", ".unity", ".prefab", ".asset", ".shader", ".hlsl",
        ".asmdef", ".asmref", ".dll", ".pdb",
    };

    readonly Dictionary<string, string> _registered = new();
    readonly List<string> _warnings = new();

    public string Register(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return null;
        if (_registered.TryGetValue(assetPath, out var existing)) return existing;

        var ext = Path.GetExtension(assetPath);
        if (ExcludedExtensions.Contains(ext)) return null;
        if (!IncludedExtensions.Contains(ext))
        {
            _warnings.Add($"Extensao nao mapeada, ignorada: {assetPath}");
            return null;
        }

        var fullPath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            assetPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (!File.Exists(fullPath))
        {
            _warnings.Add($"Asset nao encontrado no disco: {assetPath}");
            return null;
        }

        var destSubPath = BuildDestPath(assetPath, ext);
        _registered[assetPath] = destSubPath;
        return destSubPath;
    }

    public void RegisterFromJson(string jsonContent)
    {
        if (string.IsNullOrEmpty(jsonContent)) return;
        const string marker = "\"__assetPath\": \"";
        int idx = 0;
        while (true)
        {
            idx = jsonContent.IndexOf(marker, idx, StringComparison.Ordinal);
            if (idx < 0) break;
            idx += marker.Length;
            var end = jsonContent.IndexOf('"', idx);
            if (end < 0) break;
            var path = jsonContent.Substring(idx, end - idx)
                .Replace("\\/", "/").Replace("\\\\", "\\");
            Register(path);
            idx = end + 1;
        }
    }

    public void CopyAllTo(string stagingDir)
    {
        int copied = 0, failed = 0;
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;

        foreach (var (assetPath, destSubPath) in _registered)
        {
            try
            {
                var src  = Path.Combine(projectRoot, assetPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                var dest = Path.Combine(stagingDir, destSubPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(src, dest, overwrite: true);
                copied++;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AssetTracker] Falha ao copiar {assetPath}: {e.Message}");
                failed++;
            }
        }

        foreach (var w in _warnings)
            Debug.LogWarning($"[AssetTracker] {w}");

        Debug.Log($"[AssetTracker] Assets copiados: {copied}"
            + (failed > 0 ? $", falhas: {failed}" : "")
            + (Registered == 0 ? " (nenhum asset registrado)" : ""));
    }

    public string ToManifestJson()
    {
        if (_registered.Count == 0) return "[]";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[");
        var entries = new List<(string src, string dest)>();
        foreach (var kv in _registered) entries.Add((kv.Key, kv.Value));
        for (int i = 0; i < entries.Count; i++)
        {
            var comma = i < entries.Count - 1 ? "," : "";
            sb.AppendLine($"    {{ \"source\": \"{entries[i].src}\", \"dest\": \"{entries[i].dest}\" }}{comma}");
        }
        sb.Append("  ]");
        return sb.ToString();
    }

    public int Registered => _registered.Count;

    static string BuildDestPath(string assetPath, string ext)
    {
        var relative = assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
            ? assetPath.Substring(7) : assetPath;

        var folder = ext.ToLowerInvariant() switch
        {
            ".wav" or ".ogg" or ".mp3" or ".aiff"                               => "assets/audio",
            ".png" or ".jpg" or ".jpeg" or ".tga" or ".exr" or ".hdr" or ".psd" => "assets/textures",
            ".fbx" or ".obj"                                                     => "assets/models",
            ".anim"                                                               => "assets/animations",
            ".mat"                                                                => "assets/materials",
            ".ttf" or ".otf"                                                     => "assets/fonts",
            ".json" or ".xml" or ".csv"                                          => "assets/data",
            _                                                                    => "assets/misc"
        };

        return $"{folder}/{Path.GetFileName(relative)}";
    }
}
#endif