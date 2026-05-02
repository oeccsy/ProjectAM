using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SplineData
{
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public Vector3 p4;
}

public partial class Spline : MonoBehaviour
{
    [field: SerializeField]
    private List<SplineData> curveData = new List<SplineData>();
    public List<SplineData> CurveData => curveData;
    [field: SerializeField]
    public Color Color { get; set; }
    [field: SerializeField]
    public float Thickness { get; set; }

    public event Action<Vector3, Vector3, Vector3, Vector3> OnCurveAdded;

    private void Awake()
    {
        Color = Color.white;
        Thickness = 1f;
    }
    private void Reset()
    {
        Color = Color.white;
        Thickness = 1f;

        SplineData initData = new SplineData
        {
            p1 = new Vector3(-1, -1, 0),
            p2 = new Vector3(-1, 1, 0),
            p3 = new Vector3(1, 1, 0),
            p4 = new Vector3(1, -1, 0)
        };

        AddCurve(initData);
    }

    public void AddCurve(SplineData data)
    {
        curveData.Add(data);
        OnCurveAdded?.Invoke(data.p1, data.p2, data.p3, data.p4);
    }
    public void AddCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        SplineData data = new SplineData
        {
            p1 = p1,
            p2 = p2,
            p3 = p3,
            p4 = p4
        };

        curveData.Add(data);
        OnCurveAdded?.Invoke(p1, p2, p3, p4);
    }

    public Vector3 GetBezierCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float value)
    {
        Vector3 m1 = Vector3.Lerp(p1, p2, value);
        Vector3 m2 = Vector3.Lerp(p2, p3, value);
        Vector3 m3 = Vector3.Lerp(p3, p4, value);

        Vector3 m4 = Vector3.Lerp(m1, m2, value);
        Vector3 m5 = Vector3.Lerp(m2, m3, value);

        Vector3 m6 = Vector3.Lerp(m4, m5, value);

        return m6;
    }
}