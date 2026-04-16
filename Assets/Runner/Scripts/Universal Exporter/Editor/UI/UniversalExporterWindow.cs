#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UniversalExporterWindow : EditorWindow
{
    List<IExporter> _availableExporters;
    Dictionary<IExporter, bool> _toggles;
    Vector2 _scroll;

    [MenuItem("Tools/Universal Exporter", priority = 0)]
    public static void Open()
    {
        var win = GetWindow<UniversalExporterWindow>(false, "Universal Exporter", true);
        win.minSize = new Vector2(300, 400);
        win.Show();
    }

    void OnEnable()
    {
        _availableExporters = MasterExporter.GetActiveExporters();
        _toggles = _availableExporters.ToDictionary(e => e, e => true);
    }

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, 54), new Color(0.13f, 0.14f, 0.18f));
        GUI.Label(new Rect(0, 8, position.width, 24), "Universal Exporter", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState { textColor = Color.white } });

        GUILayout.Space(60);

        if (GUILayout.Button("Selecionar Tudo")) SetAll(true);
        if (GUILayout.Button("Desmarcar Tudo")) SetAll(false);

        GUILayout.Space(10);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, "box");

        foreach (var exporter in _availableExporters)
        {
            _toggles[exporter] = EditorGUILayout.ToggleLeft($"Exportar {exporter.ModuleName.ToUpper()}", _toggles[exporter]);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.Space(10);

        var selected = _availableExporters.Where(e => _toggles[e]).ToList();
        GUI.enabled = selected.Count > 0;

        if (GUILayout.Button("Exportar Cena Atual (ZIP)", GUILayout.Height(30)))
            _ = MasterExporter.RunExport(ExportScope.CurrentScene, selected);

        GUILayout.Space(5);

        if (GUILayout.Button("Exportar Projeto Inteiro (ZIP)", GUILayout.Height(30)))
            _ = MasterExporter.RunExport(ExportScope.FullProject, selected);

        GUI.enabled = true;
    }

    void SetAll(bool state)
    {
        foreach (var key in _toggles.Keys.ToList())
            _toggles[key] = state;
    }
}
#endif