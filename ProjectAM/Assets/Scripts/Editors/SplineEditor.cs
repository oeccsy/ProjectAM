#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
    private void OnSceneGUI()
    {
        Spline Generator = (Spline)target;

        if (Generator.CurveData == null) return;

        for (int i = 0; i < Generator.CurveData.Count; i++)
        {
            SplineData data = Generator.CurveData[i];

            EditorGUI.BeginChangeCheck();

            Vector3 p1 = Handles.PositionHandle(data.p1, Quaternion.identity);
            Vector3 p2 = Handles.PositionHandle(data.p2, Quaternion.identity);
            Vector3 p3 = Handles.PositionHandle(data.p3, Quaternion.identity);
            Vector3 p4 = Handles.PositionHandle(data.p4, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Generator, "Move Spline Point");
                data.p1 = p1;
                data.p2 = p2;
                data.p3 = p3;
                data.p4 = p4;
                Generator.CurveData[i] = data;
            }

            Handles.DrawLine(data.p1, data.p2);
            Handles.DrawLine(data.p3, data.p4);

            int count = 50;
            for (int j = 0; j < count; j++)
            {
                float beforeValue = (float)j / count;
                Vector3 before = Generator.GetBezierCurvePoint(data.p1, data.p2, data.p3, data.p4, beforeValue);

                float afterValue = (float)(j + 1) / count;
                Vector3 after = Generator.GetBezierCurvePoint(data.p1, data.p2, data.p3, data.p4, afterValue);

                Handles.DrawLine(before, after);
            }
        }
    }
}
#endif