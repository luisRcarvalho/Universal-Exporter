#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public interface IExporter
{
    string ModuleName { get; }

    int Order { get; }

    Task ExportProject(ExportContext ctx);
    
    Task ExportScene(Scene scene, ExportContext ctx);
}
#endif