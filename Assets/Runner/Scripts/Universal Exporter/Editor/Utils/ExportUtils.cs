#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class ExportUtils
{
    public static string Pad(int n) => new string(' ', n);
    public static string F(float v) => v.ToString("G", CultureInfo.InvariantCulture);
    public static string J4(float v) => v.ToString("F4", CultureInfo.InvariantCulture);
    public static string B(bool v)  => v ? "true" : "false";

    public static string Esc(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    public static string V2(Vector2 v) => $"{{ \"x\": {F(v.x)}, \"y\": {F(v.y)} }}";
    public static string V3(Vector3 v) => $"{{ \"x\": {F(v.x)}, \"y\": {F(v.y)}, \"z\": {F(v.z)} }}";
    public static string ColorJson(Color c) => $"{{ \"r\": {F(c.r)}, \"g\": {F(c.g)}, \"b\": {F(c.b)}, \"a\": {F(c.a)} }}";

    public static string GetHierarchyPath(Transform t)
    {
        var parts = new List<string>();
        var cur = t;
        while (cur != null)
        {
            parts.Insert(0, cur.gameObject.name);
            cur = cur.parent;
        }
        return string.Join("/", parts);
    }
}
#endif