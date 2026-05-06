using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Line), typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadLineRenderer : MonoBehaviour
{
    private Line line;
    private MeshFilter meshFilter;

    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector3> uv0 = new List<Vector3>();
    private readonly List<Vector3> uv1 = new List<Vector3>(); 
    
    private void Awake()
    {
        line = GetComponent<Line>();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        
        Material material = new Material(Shader.Find("Line/QuadLineShader"));
        material.SetColor("_Color", line.Color);
        material.SetFloat("_Thickness", line.Thickness);

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        
        for(int i=0; i<line.Points.Count; i+=2)
        {
            AddQuad(line.Points[i], line.Points[i+1]);
        }

        line.OnLineAdded += AddQuad;
    }

    private void AddQuad(Vector3 start, Vector3 end)
    {
        float thickness = line.Thickness;
        float minX = Mathf.Min(start.x, end.x) - thickness;
        float minY = Mathf.Min(start.y, end.y) - thickness;
        float maxX = Mathf.Max(start.x, end.x) + thickness;
        float maxY = Mathf.Max(start.y, end.y) + thickness;
        
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
            uv0.Add(start);
            uv1.Add(end);
        }

        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uv0);
        mesh.SetUVs(1, uv1);
        mesh.RecalculateBounds();
    }
}
