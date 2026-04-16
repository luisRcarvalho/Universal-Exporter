#if UNITY_EDITOR
using UnityEngine.SceneManagement;

public interface IExporter
{
    string ModuleName { get; }

    int Order { get; }

    void ExportProject(ExportContext ctx);
    
    void ExportScene(Scene scene, ExportContext ctx);
}
#endif