#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PostProcessingVolumeExporter : IExporter
{
    public string ModuleName => "postprocessing";
    public int Order => 54;

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var allVolumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        var validVolumes = new List<Volume>();
        
        // Filtra para garantir que só exportaremos volumes com profiles reais
        foreach (var v in allVolumes)
        {
            if (v.profile != null) validVolumes.Add(v);
        }

        if (validVolumes.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"volumes\": [");

        for (int i = 0; i < validVolumes.Count; i++)
        {
            var volume = validVolumes[i];
            var commaVol = i < validVolumes.Count - 1 ? "," : "";
            
            sb.AppendLine("    {");
            
            var components = volume.profile.components;
            
            sb.AppendLine($"      \"_meta\": {{ \"profile\": \"{ExportUtils.Esc(volume.profile.name)}\" }}" + (components.Count > 0 ? "," : ""));

            for (int c = 0; c < components.Count; c++)
            {
                var comp = components[c];
                if (comp == null) continue;
                
                var comma = c < components.Count - 1 ? "," : "";
                sb.AppendLine($"      \"{comp.GetType().Name}\": {{");
                sb.AppendLine($"        \"active\": {ExportUtils.B(comp.active)},");
                
                var fields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                var fList = new List<string>();
                foreach (var f in fields)
                {
                    if (typeof(VolumeParameter).IsAssignableFrom(f.FieldType))
                    {
                        var param = f.GetValue(comp) as VolumeParameter;
                        object valObj = null;

                        if (param != null)
                        {
                            var pType = param.GetType();
                            var vProp = pType.GetProperty("value");
                            var vField = pType.GetField("value");
                            
                            if (vProp != null) valObj = vProp.GetValue(param);
                            else if (vField != null) valObj = vField.GetValue(param);
                        }

                        var valStr = valObj?.ToString() ?? "null";
                        
                        if (valObj is bool b) valStr = ExportUtils.B(b);
                        else if (valObj is float fl) valStr = ExportUtils.F(fl);
                        else if (valObj is Color col) valStr = ExportUtils.ColorJson(col);
                        else if (valObj is int iVal) valStr = iVal.ToString();
                        
                        fList.Add($"        \"{f.Name}\": \"{ExportUtils.Esc(valStr)}\"");
                    }
                }
                sb.AppendLine(string.Join(",\n", fList));
                sb.AppendLine($"      }}{comma}");
            }
            sb.AppendLine($"    }}{commaVol}");
        }
        
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        
        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"postprocess_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        
        Debug.Log($"[PPExporter] {validVolumes.Count} volumes exportados da cena {scene.name}.");
        await Task.CompletedTask;
    }
}
#endif