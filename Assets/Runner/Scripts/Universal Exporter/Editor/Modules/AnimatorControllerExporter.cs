#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimatorControllerExporter : IExporter
{
    public string ModuleName => "animation";
    public int Order => 53; // CORRIGIDO!

    public async Task ExportProject(ExportContext ctx) { await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
        var animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        var exported = new HashSet<string>();
        int count = 0;

        foreach (var anim in animators)
        {
            var ctrl = anim.runtimeAnimatorController as AnimatorController;
            if (ctrl == null || !exported.Add(ctrl.name)) continue;

            ExportController(ctrl, ctx.EnsureDir(ModuleName));
            count++;
        }

        if (count > 0)
        {
            Debug.Log($"[AnimExporter] {count} controller(s) exportado(s).");
        }
        await Task.CompletedTask;
    }

    private void ExportController(AnimatorController ctrl, string outDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"name\": \"{ExportUtils.Esc(ctrl.name)}\",");
        
        sb.AppendLine("  \"layers\": [");
        for (int i = 0; i < ctrl.layers.Length; i++)
        {
            var layer = ctrl.layers[i];
            var sm = layer.stateMachine;
            var lc = i < ctrl.layers.Length - 1 ? "," : "";
            
            sb.AppendLine("    {");
            sb.AppendLine($"      \"name\": \"{ExportUtils.Esc(layer.name)}\",");
            sb.AppendLine($"      \"weight\": {ExportUtils.F(layer.defaultWeight)},");
            sb.AppendLine($"      \"defaultState\": \"{ExportUtils.Esc(sm.defaultState?.name)}\",");
            
            sb.AppendLine("      \"states\": [");
            for (int j = 0; j < sm.states.Length; j++)
            {
                var st = sm.states[j].state;
                var sc = j < sm.states.Length - 1 ? "," : "";
                sb.AppendLine("        {");
                sb.AppendLine($"          \"name\": \"{ExportUtils.Esc(st.name)}\",");
                sb.AppendLine($"          \"speed\": {ExportUtils.F(st.speed)},");
                
                sb.AppendLine("          \"transitions\": [");
                for (int tIdx = 0; tIdx < st.transitions.Length; tIdx++)
                {
                    var t = st.transitions[tIdx];
                    var tc = tIdx < st.transitions.Length - 1 ? "," : "";
                    sb.AppendLine($"            {{ \"to\": \"{ExportUtils.Esc(t.destinationState?.name ?? "Exit")}\", \"conditions\": [{SerializeConditions(t.conditions)}] }}{tc}");
                }
                sb.AppendLine("          ]");
                sb.AppendLine($"        }}{sc}");
            }
            sb.AppendLine("      ],");
            
            sb.AppendLine("      \"anyStateTransitions\": [");
            for (int tIdx = 0; tIdx < sm.anyStateTransitions.Length; tIdx++)
            {
                var t = sm.anyStateTransitions[tIdx];
                var tc = tIdx < sm.anyStateTransitions.Length - 1 ? "," : "";
                sb.AppendLine($"        {{ \"to\": \"{ExportUtils.Esc(t.destinationState?.name ?? "Exit")}\", \"conditions\": [{SerializeConditions(t.conditions)}] }}{tc}");
            }
            sb.AppendLine("      ]");
            sb.AppendLine($"    }}{lc}");
        }
        sb.AppendLine("  ],");
        
        sb.AppendLine("  \"parameters\": [");
        for (int i = 0; i < ctrl.parameters.Length; i++)
        {
            var p = ctrl.parameters[i];
            var comma = i < ctrl.parameters.Length - 1 ? "," : "";
            sb.AppendLine($"    {{ \"name\": \"{ExportUtils.Esc(p.name)}\", \"type\": \"{p.type}\", \"default\": {GetDefaultValue(p)} }}{comma}");
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        File.WriteAllText(Path.Combine(outDir, $"AnimatorController_{ctrl.name}.json"), sb.ToString(), Encoding.UTF8);
    }

    private string SerializeConditions(AnimatorCondition[] conds)
    {
        var parts = new List<string>();
        foreach (var c in conds)
        {
            parts.Add($"{{ \"parameter\": \"{ExportUtils.Esc(c.parameter)}\", \"mode\": \"{c.mode}\", \"threshold\": {ExportUtils.F(c.threshold)} }}");
        }
        return string.Join(", ", parts);
    }

    private string GetDefaultValue(AnimatorControllerParameter p)
    {
        switch (p.type)
        {
            case AnimatorControllerParameterType.Float: return ExportUtils.F(p.defaultFloat);
            case AnimatorControllerParameterType.Int: return p.defaultInt.ToString();
            case AnimatorControllerParameterType.Bool: return ExportUtils.B(p.defaultBool);
            default: return "false";
        }
    }
}
#endif