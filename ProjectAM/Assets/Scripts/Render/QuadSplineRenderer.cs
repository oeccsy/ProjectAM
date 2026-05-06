using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Spline), typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadSplineRenderer : MonoBehaviour
{
    private Spline spline;
    private MeshFilter meshFilter;

    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector3> uv0 = new List<Vector3>();
    private readonly List<Vector3> uv1 = new List<Vector3>();
    private readonly List<Vector3> uv2 = new List<Vector3>();
    private readonly List<Vector3> uv3 = new List<Vector3>();

    private void Awake()
    {
        spline = GetComponent<Spline>();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();

        Material material = new Material(Shader.Find("Spline/QuadSplineShader"));
        material.SetColor("_Color", spline.Color);
        material.SetFloat("_Thickness", spline.Thickness);

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        foreach (SplineData curve in spline.CurveData)
        {
            AddQuad(curve);
        }

        spline.OnCurveAdded += AddQuad;
    }

    private void OnDestroy()
    {
        spline.OnCurveAdded -= AddQuad;
    }

    private void AddQuad(SplineData curve)
    {
        float thickness  = spline.Thickness;
        float minX = Mathf.Min(curve.p1.x, curve.p2.x, curve.p3.x, curve.p4.x) - thickness;
        float minY = Mathf.Min(curve.p1.y, curve.p2.y, curve.p3.y, curve.p4.y) - thickness;
        float maxX = Mathf.Max(curve.p1.x, curve.p2.x, curve.p3.x, curve.p4.x) + thickness;
        float maxY = Mathf.Max(curve.p1.y, curve.p2.y, curve.p3.y, curve.p4.y) + thickness;

        int baseIndex = vertices.Count;

        vertices.Add(new Vector3(minX, minY, 0f));
        vertices.Add(new Vector3(minX, maxY, 0f));
        vertices.Add(new Vector3(maxX, maxY, 0f));
        vertices.Add(new Vector3(maxX, minY, 0f));

        triangles.Add(baseIndex + 0);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 0);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);

        // 4개 버텍스 모두 동일한 제어점을 가지도록 반복
        for (int i = 0; i < 4; i++)
        {
            uv0.Add(curve.p1);
            uv1.Add(curve.p2);
            uv2.Add(curve.p3);
            uv3.Add(curve.p4);
        }

        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uv0);
        mesh.SetUVs(1, uv1);
        mesh.SetUVs(2, uv2);
        mesh.SetUVs(3, uv3);
        mesh.RecalculateBounds();
    }
}
