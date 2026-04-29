#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

// Importa o glTFast de verdade se a tag estiver ativada na Unity
#if GLTFAST_INSTALLED
using GLTFast.Export;
#endif

public class GltfSceneExporter : IExporter
{
    public string ModuleName => "gltf_scene";
    public int Order => 95; // Executa por último

    public async Task ExportProject(ExportContext ctx) {await Task.CompletedTask; }

    public async Task ExportScene(Scene scene, ExportContext ctx)
    {
#if GLTFAST_INSTALLED
        string fileName = $"{scene.name}.glb";
        string filePath = Path.Combine(ctx.StagingDir, fileName);

        try
        {
            var settings = new ExportSettings { Format = GltfFormat.Binary, FileConflictResolution = FileConflictResolution.Overwrite };
            var exporter = new GameObjectExport(settings);
            exporter.AddScene(scene.GetRootGameObjects(), scene.name);
            
            await exporter.SaveToFileAndDispose(filePath); 

            if (File.Exists(filePath))
                Debug.Log($"[GltfExporter] SUCESSO! GLB gerado na raiz do ZIP: {fileName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GltfExporter] Erro: {e.Message}");
        }
#else
        Debug.LogWarning("[GltfExporter] Tag GLTFAST_INSTALLED ausente.");
        await Task.CompletedTask;
#endif
    }
}
#endif