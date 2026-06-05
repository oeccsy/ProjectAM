#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class MeasureAssetBounds
{
    [MenuItem("Tools/Measure Asset Bounds")]
    public static void Measure()
    {
        string[] paths = {
            "Assets/Resources/Ground/ground_steppingpillar_A_1x1_v1.glb",
            "Assets/Resources/NPC/Character_01.glb",
            "Assets/Resources/Structure/Structures_Cabin_A_S_v01.glb",
            "Assets/Resources/Structure/Structures_Cabin_B_M_v01.glb",
        };

        var sb = new System.Text.StringBuilder();
        foreach (var path in paths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) { sb.AppendLine(path + ": NOT FOUND"); continue; }

            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) { sb.AppendLine(path + ": no renderers"); continue; }

            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);

            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            sb.AppendLine("[" + name + "]");
            sb.AppendLine("  size:   " + b.size.x.ToString("F3") + " x " + b.size.y.ToString("F3") + " x " + b.size.z.ToString("F3"));
            sb.AppendLine("  center: " + b.center.x.ToString("F3") + " x " + b.center.y.ToString("F3") + " x " + b.center.z.ToString("F3"));
        }

        string result = sb.ToString();
        File.WriteAllText("Assets/bounds_result.txt", result);
        Debug.Log("[MeasureAssetBounds]\n" + result);
        AssetDatabase.Refresh();
    }
}
#endif
