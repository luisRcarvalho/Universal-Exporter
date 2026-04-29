#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
public class ExportProfile : ScriptableObject
{
    public List<string> ActiveModules = new List<string>();
}
#endif