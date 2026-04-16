#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class HtmlReportGenerator
{
    const string INJECT_START = "";
    const string INJECT_END   = "";

    public static void Generate(string stagingDir, string outputDir)
    {
        var templatePath = FindTemplate();
        if (templatePath == null)
        {
            Debug.LogWarning("[HtmlReport] unity-inspector.html não encontrado. Certifique-se de que o arquivo está no projeto.");
            return;
        }

        var template = File.ReadAllText(templatePath, Encoding.UTF8);
        var jsonFiles = CollectJsonFiles(stagingDir);

        if (jsonFiles.Count == 0)
        {
            Debug.LogWarning("[HtmlReport] Nenhum JSON encontrado para gerar o relatório.");
            return;
        }

        var injection = BuildInjection(jsonFiles);
        var result    = InjectIntoTemplate(template, injection);
        
        var outPath = Path.Combine(outputDir, 
            "universal_inspector_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html");

        File.WriteAllText(outPath, result, Encoding.UTF8);
        Debug.Log($"[HtmlReport] Relatório visual gerado: {outPath}");
    }

    static List<string> CollectJsonFiles(string dir)
    {
        var list = new List<string>();
        if (!Directory.Exists(dir)) return list;
        list.AddRange(Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories));
        return list;
    }

    static string BuildInjection(List<string> files)
    {
        var sb = new StringBuilder();
        sb.AppendLine(INJECT_START);
        sb.AppendLine("<script id=\"universal-export-data\" type=\"application/json\">");
        sb.AppendLine("{");
        sb.AppendLine("  \"files\": [");

        for (int i = 0; i < files.Count; i++)
        {
            var path = files[i];
            var name = Path.GetFileName(path);
            var content = File.ReadAllText(path);
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
            
            var comma = i < files.Count - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{name}\", \"content\": \"{b64}\" }}{comma}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");
        sb.AppendLine("</script>");
        sb.Append(INJECT_END);
        return sb.ToString();
    }

    static string InjectIntoTemplate(string template, string injection)
    {
        var startIdx = template.IndexOf(INJECT_START, StringComparison.Ordinal);
        var endIdx   = template.IndexOf(INJECT_END,   StringComparison.Ordinal);

        if (startIdx >= 0 && endIdx > startIdx)
        {
            return template.Substring(0, startIdx) + injection + template.Substring(endIdx + INJECT_END.Length);
        }

        return template.Replace("</head>", injection + "\n</head>");
    }

    static string FindTemplate()
    {
        string[] searchTerms = { "universal-inspector", "unity-inspector" };
        
        foreach (var term in searchTerms)
        {
            string[] guids = AssetDatabase.FindAssets(term);
            
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.ToLower().EndsWith(".html"))
                {
                    string projectRoot = Path.GetDirectoryName(Application.dataPath);
                    return Path.GetFullPath(Path.Combine(projectRoot, path));
                }
            }
        }
        
        return null;
    }
}
#endif