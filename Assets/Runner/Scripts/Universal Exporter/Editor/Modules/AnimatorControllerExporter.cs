#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class AnimatorControllerExporter : IExporter
{
    public string ModuleName => "animation";
    public int Order => 53; 

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var animators = UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        var exportedNames = new HashSet<string>();
        var controllersToExport = new List<AnimatorController>();

        foreach (var anim in animators)
        {
            var ctrl = anim.runtimeAnimatorController as AnimatorController;
            if (ctrl != null && exportedNames.Add(ctrl.name))
            {
                controllersToExport.Add(ctrl);
            }
        }

        if (controllersToExport.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"scene\": \"{ExportUtils.Esc(scene.name)}\",");
        sb.AppendLine("  \"animatorControllers\": [");

        for (int i = 0; i < controllersToExport.Count; i++)
        {
            var ctrl = controllersToExport[i];
            var comma = i < controllersToExport.Count - 1 ? "," : "";
            
            sb.AppendLine("    {");
            sb.AppendLine($"      \"name\": \"{ExportUtils.Esc(ctrl.name)}\",");
            sb.AppendLine("      \"layers\": [");
            for (int j = 0; j < ctrl.layers.Length; j++)
            {
                var layer = ctrl.layers[j];
                var sm = layer.stateMachine;
                var lc = j < ctrl.layers.Length - 1 ? "," : "";
                sb.AppendLine("        {");
                sb.AppendLine($"          \"name\": \"{ExportUtils.Esc(layer.name)}\",");
                sb.AppendLine($"          \"weight\": {ExportUtils.F(layer.defaultWeight)},");
                
                // Extraído para evitar erro de aspas na string interpolada
                string defStateName = sm.defaultState != null ? sm.defaultState.name : "";
                sb.AppendLine($"          \"defaultState\": \"{ExportUtils.Esc(defStateName)}\",");
                
                sb.AppendLine("          \"states\": [");
                for (int sIdx = 0; sIdx < sm.states.Length; sIdx++)
                {
                    var st = sm.states[sIdx].state;
                    var sc = sIdx < sm.states.Length - 1 ? "," : "";
                    sb.AppendLine("            {");
                    sb.AppendLine($"              \"name\": \"{ExportUtils.Esc(st.name)}\",");
                    sb.AppendLine($"              \"speed\": {ExportUtils.F(st.speed)},");
                    sb.AppendLine("              \"transitions\": [");
                    for (int tIdx = 0; tIdx < st.transitions.Length; tIdx++)
                    {
                        var t = st.transitions[tIdx];
                        var tc = tIdx < st.transitions.Length - 1 ? "," : "";
                        
                        // O cálculo do destino resolvido de forma limpa
                        string destName = t.destinationState != null ? t.destinationState.name : "Exit";
                        sb.AppendLine($"                {{ \"to\": \"{ExportUtils.Esc(destName)}\", \"conditions\": [{SerializeConditions(t.conditions)}] }}{tc}");
                    }
                    sb.AppendLine("              ]");
                    sb.AppendLine($"            }}{sc}");
                }
                sb.AppendLine("          ],");
                sb.AppendLine("          \"anyStateTransitions\": [");
                for (int tIdx = 0; tIdx < sm.anyStateTransitions.Length; tIdx++)
                {
                    var t = sm.anyStateTransitions[tIdx];
                    var tc = tIdx < sm.anyStateTransitions.Length - 1 ? "," : "";
                    
                    // O cálculo do destino resolvido de forma limpa
                    string destName = t.destinationState != null ? t.destinationState.name : "Exit";
                    sb.AppendLine($"            {{ \"to\": \"{ExportUtils.Esc(destName)}\", \"conditions\": [{SerializeConditions(t.conditions)}] }}{tc}");
                }
                sb.AppendLine("          ]");
                sb.AppendLine($"        }}{lc}");
            }
            sb.AppendLine("      ],");
            sb.AppendLine("      \"parameters\": [");
            for (int pIdx = 0; pIdx < ctrl.parameters.Length; pIdx++)
            {
                var p = ctrl.parameters[pIdx];
                var pc = pIdx < ctrl.parameters.Length - 1 ? "," : "";
                sb.AppendLine($"        {{ \"name\": \"{ExportUtils.Esc(p.name)}\", \"type\": \"{p.type}\", \"default\": {GetDefaultValue(p)} }}{pc}");
            }
            sb.AppendLine("      ]");
            sb.AppendLine($"    }}{comma}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(ctx.EnsureDir(ModuleName), $"animation_{scene.name}.json"), sb.ToString(), Encoding.UTF8);
        await Task.CompletedTask;
    }

    private string SerializeConditions(AnimatorCondition[] conds)
    {
        var parts = new List<string>();
        foreach (var c in conds)
            parts.Add($"{{ \"parameter\": \"{ExportUtils.Esc(c.parameter)}\", \"mode\": \"{c.mode}\", \"threshold\": {ExportUtils.F(c.threshold)} }}");
        return string.Join(", ", parts);
    }

    private string GetDefaultValue(AnimatorControllerParameter p)
    {
        switch (p.type) {
            case AnimatorControllerParameterType.Float: return ExportUtils.F(p.defaultFloat);
            case AnimatorControllerParameterType.Int: return p.defaultInt.ToString();
            case AnimatorControllerParameterType.Bool: return ExportUtils.B(p.defaultBool);
            default: return "false";
        }
    }
}
#endif