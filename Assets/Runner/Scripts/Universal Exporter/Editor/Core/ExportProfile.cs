#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewExportProfile", menuName = "Universal Exporter/Export Profile")]
public class ExportProfile : ScriptableObject
{
    public List<string> activeModules = new List<string>();
}
#endif