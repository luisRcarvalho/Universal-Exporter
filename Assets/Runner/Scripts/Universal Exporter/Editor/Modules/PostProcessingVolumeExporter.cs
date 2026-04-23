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
    public int Order => 54; // CORRIGIDO!

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        if (volumes.Length == 0) return;

        foreach (var volume in volumes)
        {
            if (volume.profile == null) continue;
            
            var sb = new StringBuilder();
            sb.AppendLine("{");
            
            var components = volume.profile.components;
            
            // Correção de vírgula para manter o JSON blindado se não houver componentes
            sb.AppendLine($"  \"_meta\": {{ \"profile\": \"{ExportUtils.Esc(volume.profile.name)}\" }}" + (components.Count > 0 ? "," : ""));

            for (int i = 0; i < components.Count; i++)
            {
                var comp = components[i];
                if (comp == null) continue;
                
                var comma = i < components.Count - 1 ? "," : "";
                sb.AppendLine($"  \"{comp.GetType().Name}\": {{");
                sb.AppendLine($"    \"active\": {ExportUtils.B(comp.active)},");
                
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
                        else if (valObj is Color c) valStr = ExportUtils.ColorJson(c);
                        else if (valObj is int iVal) valStr = iVal.ToString();
                        
                        fList.Add($"    \"{f.Name}\": \"{ExportUtils.Esc(valStr)}\"");
                    }
                }
                sb.AppendLine(string.Join(",\n", fList));
                sb.AppendLine($"  }}{comma}");
            }
            sb.AppendLine("}");

            File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"postprocess_{volume.gameObject.name}.json"), sb.ToString(), Encoding.UTF8);
        }
        
        Debug.Log($"[PPExporter] {volumes.Length} volumes exportados.");
        await Task.CompletedTask;
    }
}
#endif