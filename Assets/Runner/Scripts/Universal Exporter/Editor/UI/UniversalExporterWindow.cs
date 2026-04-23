#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ExportProfile : ScriptableObject
{
    public List<string> ActiveModules = new List<string>();
}

public class UniversalExporterWindow : EditorWindow
{
    private Vector2 _mainScroll;
    private List<IExporter> _allExporters = new List<IExporter>();
    private Dictionary<IExporter, bool> _exporterSelection = new Dictionary<IExporter, bool>();
    private Dictionary<string, bool> _sceneSelection = new Dictionary<string, bool>();
    private ExportProfile _selectedProfile;

    [MenuItem("Tools/Universal Exporter")]
    public static void ShowWindow()
    {
        var window = GetWindow<UniversalExporterWindow>("Universal Exporter");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnEnable()
    {
        LoadExporters();
        LoadScenesFromBuild();
    }

    private void LoadExporters()
    {
        _allExporters.Clear();
        _exporterSelection.Clear();

        var types = TypeCache.GetTypesDerivedFrom<IExporter>();
        foreach (var t in types)
        {
            if (!t.IsAbstract && !t.IsInterface)
            {
                var exporter = (IExporter)Activator.CreateInstance(t);
                _allExporters.Add(exporter);
            }
        }

        _allExporters = _allExporters.OrderBy(e => e.Order).ToList();
        foreach (var exp in _allExporters) _exporterSelection[exp] = true;
    }

    private void LoadScenesFromBuild()
    {
        _sceneSelection.Clear();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled) _sceneSelection[scene.path] = true;
        }
    }

    private string FormatName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return rawName;
        var words = rawName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
        }
        return string.Join(" ", words);
    }

    private void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(10, 10, 15, 15)
        };

        _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);

        GUILayout.Label("Universal Exporter", titleStyle);
        EditorGUILayout.Space(5);

        DrawPresetsSection();
        EditorGUILayout.Space(5);
        DrawExportersSection();
        EditorGUILayout.Space(15);
        DrawScenesSection();

        EditorGUILayout.EndScrollView();

        DrawSeparator();
        DrawExportButtons();
    }

    private void DrawPresetsSection()
    {
        GUILayout.Label("Predefinições de Exportação (Presets)", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        _selectedProfile = (ExportProfile)EditorGUILayout.ObjectField("Usar Preset:", _selectedProfile, typeof(ExportProfile), false);
        if (EditorGUI.EndChangeCheck() && _selectedProfile != null)
        {
            ApplyPreset(_selectedProfile);
        }

        if (GUILayout.Button("Salvar Marcações Atuais como Preset"))
        {
            SaveCurrentAsPreset();
        }
    }

    private void DrawExportersSection()
    {
        EditorGUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Selecionar Tudo"))
        {
            foreach (var key in _exporterSelection.Keys.ToList()) _exporterSelection[key] = true;
        }
        if (GUILayout.Button("Desmarcar Tudo"))
        {
            foreach (var key in _exporterSelection.Keys.ToList()) _exporterSelection[key] = false;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        foreach (var exp in _allExporters)
        {
            string displayName = $"Exportar {FormatName(exp.ModuleName)}";
            _exporterSelection[exp] = EditorGUILayout.ToggleLeft(displayName, _exporterSelection[exp]);
        }
    }

    private void DrawScenesSection()
    {
        GUILayout.Label("Cenas para Exportar", EditorStyles.boldLabel);
        
        var paths = new List<string>(_sceneSelection.Keys);
        if (paths.Count == 0) 
        {
            GUILayout.Label("Nenhuma cena ativada no Build Settings.");
        }
        else
        {
            foreach (var path in paths)
            {
                string sceneName = Path.GetFileNameWithoutExtension(path);
                _sceneSelection[path] = EditorGUILayout.ToggleLeft(sceneName, _sceneSelection[path]);
            }
        }
    }

    private void DrawExportButtons()
    {
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Exportar Cena Atual (ZIP)", GUILayout.Height(30)))
        {
            string currentScene = EditorSceneManager.GetActiveScene().path;
            StartExportProcess(ExportScope.CurrentScene, new List<string> { currentScene });
        }

        EditorGUILayout.Space(2);

        if (GUILayout.Button("Exportar Projeto Inteiro (ZIP)", GUILayout.Height(30)))
        {
            var activeScenes = _sceneSelection.Where(k => k.Value).Select(k => k.Key).ToList();
            StartExportProcess(ExportScope.FullProject, activeScenes);
        }
        
        EditorGUILayout.Space(5);
    }

    private void StartExportProcess(ExportScope scope, List<string> scenes)
    {
        var activeExporters = _exporterSelection.Where(k => k.Value).Select(k => k.Key).ToList();
        
        if (activeExporters.Count == 0)
        {
            Debug.LogWarning("[Universal Exporter] Nenhum módulo selecionado!");
            return;
        }
        
        if (scenes.Count == 0 && scope == ExportScope.FullProject)
        {
            Debug.LogWarning("[Universal Exporter] Nenhuma cena selecionada!");
            return;
        }

        _ = MasterExporter.RunExport(scope, scenes, activeExporters);
    }

    private void ApplyPreset(ExportProfile profile)
    {
        foreach (var key in _exporterSelection.Keys.ToList()) _exporterSelection[key] = false;
        foreach (var module in profile.ActiveModules)
        {
            var exporter = _allExporters.FirstOrDefault(e => e.ModuleName == module);
            if (exporter != null) _exporterSelection[exporter] = true;
        }
    }

    private void SaveCurrentAsPreset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Salvar Preset", "NovoPreset", "asset", "Salve o arquivo do perfil");
        if (string.IsNullOrEmpty(path)) return;

        var profile = CreateInstance<ExportProfile>();
        profile.ActiveModules = _exporterSelection.Where(k => k.Value).Select(k => k.Key.ModuleName).ToList();

        AssetDatabase.CreateAsset(profile, path);
        AssetDatabase.SaveAssets();
        _selectedProfile = profile;
        Debug.Log($"[Universal Exporter] Preset salvo em: {path}");
    }

    private void DrawSeparator()
    {
        EditorGUILayout.Space();
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
        EditorGUILayout.Space();
    }
}
#endif