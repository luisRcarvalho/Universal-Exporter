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
    ExportProfile _currentProfile; // Guarda o Preset atual

    [MenuItem("Tools/Universal Exporter", priority = 0)]
    public static void Open()
    {
        var win = GetWindow<UniversalExporterWindow>(false, "Universal Exporter", true);
        win.minSize = new Vector2(300, 450);
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
        
        GUI.Label(new Rect(0, 8, position.width, 38), "Universal Exporter", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState { textColor = Color.white } });

        GUILayout.Space(60);
        
        GUILayout.BeginVertical("box");
        GUILayout.Label("Predefinições de Exportação (Presets)", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        _currentProfile = (ExportProfile)EditorGUILayout.ObjectField("Usar Preset:", _currentProfile, typeof(ExportProfile), false);
        
        if (EditorGUI.EndChangeCheck() && _currentProfile != null) ApplyProfile(_currentProfile);

        if (GUILayout.Button("Salvar Marcações Atuais como Preset")) CreateAndSaveProfile();
        GUILayout.EndVertical();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Selecionar Tudo")) SetAll(true);
        if (GUILayout.Button("Desmarcar Tudo")) SetAll(false);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, "box");

        foreach (var exporter in _availableExporters)
        {
            EditorGUI.BeginChangeCheck();
            
            string rawName = exporter.ModuleName.Replace("_", " ").ToLower();
            string displayName = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(rawName);

            _toggles[exporter] = EditorGUILayout.ToggleLeft($"Exportar {displayName}", _toggles[exporter]);
            
            if (EditorGUI.EndChangeCheck()) _currentProfile = null;
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
        _currentProfile = null;
        foreach (var key in _toggles.Keys.ToList())
            _toggles[key] = state;
    }

    void ApplyProfile(ExportProfile profile)
    {
        if (profile == null) return;
        foreach (var key in _toggles.Keys.ToList())
        {
            _toggles[key] = profile.activeModules.Contains(key.ModuleName);
        }
    }

    void CreateAndSaveProfile()
    {
        var profile = ScriptableObject.CreateInstance<ExportProfile>();
        profile.activeModules = _toggles.Where(kvp => kvp.Value).Select(kvp => kvp.Key.ModuleName).ToList();

        string path = EditorUtility.SaveFilePanelInProject("Salvar Preset", "NovoPresetExportacao", "asset", "Escolha onde salvar o preset");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            _currentProfile = profile;
            Debug.Log($"[Universal Exporter] Preset salvo em: {path}");
        }
    }
}
#endif