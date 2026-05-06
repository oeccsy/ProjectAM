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

public class Spline : MonoBehaviour
{
    [SerializeField]
    private List<SplineData> curveData = new List<SplineData>();
    public List<SplineData> CurveData => curveData;
    [field: SerializeField]
    public Color Color { get; set; }
    [field: SerializeField]
    public float Thickness { get; set; }

    public event Action<SplineData> OnCurveAdded;

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
        OnCurveAdded?.Invoke(data);
    }

    public Vector3 GetBezierCurvePoint(SplineData splineData, float value)
    {
        Vector3 m1 = Vector3.Lerp(splineData.p1, splineData.p2, value);
        Vector3 m2 = Vector3.Lerp(splineData.p2, splineData.p3, value);
        Vector3 m3 = Vector3.Lerp(splineData.p3, splineData.p4, value);

        Vector3 m4 = Vector3.Lerp(m1, m2, value);
        Vector3 m5 = Vector3.Lerp(m2, m3, value);

        Vector3 m6 = Vector3.Lerp(m4, m5, value);

        return m6;
    }
}