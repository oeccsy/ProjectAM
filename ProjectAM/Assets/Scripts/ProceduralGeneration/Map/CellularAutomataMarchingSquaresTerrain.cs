using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CellularAutomataMarchingSquaresTerrain : MonoBehaviour
{
    [SerializeField]
    private float tileSize = 1f;
    [SerializeField]
    private float topHeight = 1.0f;
    [SerializeField]
    private float bottomHeight = 0.0f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer.sharedMaterial = CreateDefaultMaterial();
    }

    public void Build()
    {
        CellularAutomata cellularAutomata = new CellularAutomata();
        CellularAutomataSettings settings = new CellularAutomataSettings
        {
            width = 128,
            height = 128,
            fillPercentage = 50,
            smoothIterations = 5,
            blurIterations = 3
        };

        float[,] cellularMap = cellularAutomata.GenerateCellularMap(settings);

        Mesh mesh = GenerateMesh(cellularMap);
        mesh.name = "Cellular Automata Marching Squares Terrain";
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        Debug.Log($"Cellular Automata Marching Squares Terrain: vertices={mesh.vertexCount}, triangles={mesh.triangles.Length / 3}");
    }

    private Mesh GenerateMesh(float[,] cellularMap)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        int height = cellularMap.GetLength(0);
        int width = cellularMap.GetLength(1);

        MarchingSquares marchingSquares = new MarchingSquares();

        for (int row = 0; row < height - 1; row++)
        {
            for (int col = 0; col < width - 1; col++)
            {
                MarchingSquares.CellData cellData = marchingSquares.Sample(cellularMap, row, col);

                for (int i = 0; i < cellData.triangles.Length; i += 3)
                {
                    Vector2 a = cellData.points[cellData.triangles[i]];
                    Vector2 b = cellData.points[cellData.triangles[i + 1]];
                    Vector2 c = cellData.points[cellData.triangles[i + 2]];

                    int topStart = vertices.Count;
                    vertices.Add(new Vector3(a.x, topHeight, a.y));
                    vertices.Add(new Vector3(b.x, topHeight, b.y));
                    vertices.Add(new Vector3(c.x, topHeight, c.y));

                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);

                    uvs.Add(new Vector2(a.x, a.y) / tileSize);
                    uvs.Add(new Vector2(b.x, b.y) / tileSize);
                    uvs.Add(new Vector2(c.x, c.y) / tileSize);

                    triangles.Add(topStart);
                    triangles.Add(topStart + 1);
                    triangles.Add(topStart + 2);

                    int bottomStart = vertices.Count;
                    vertices.Add(new Vector3(a.x, bottomHeight, a.y));
                    vertices.Add(new Vector3(b.x, bottomHeight, b.y));
                    vertices.Add(new Vector3(c.x, bottomHeight, c.y));

                    normals.Add(Vector3.down);
                    normals.Add(Vector3.down);
                    normals.Add(Vector3.down);

                    uvs.Add(new Vector2(a.x, a.y) / tileSize);
                    uvs.Add(new Vector2(b.x, b.y) / tileSize);
                    uvs.Add(new Vector2(c.x, c.y) / tileSize);

                    triangles.Add(bottomStart);
                    triangles.Add(bottomStart + 2);
                    triangles.Add(bottomStart + 1);
                }

                for (int i = 0; i < cellData.contours.Length; i += 2)
                {
                    Vector2 u = cellData.points[cellData.contours[i]];
                    Vector2 v = cellData.points[cellData.contours[i + 1]];
                    Vector2 edge = v - u;

                    if (edge.sqrMagnitude < 0.000001f) continue;

                    int sideStart = vertices.Count;
                    vertices.Add(new Vector3(u.x, topHeight, u.y));
                    vertices.Add(new Vector3(u.x, bottomHeight, u.y));
                    vertices.Add(new Vector3(v.x, bottomHeight, v.y));
                    vertices.Add(new Vector3(v.x, topHeight, v.y));

                    Vector3 normal = new Vector3(-edge.y, 0f, edge.x).normalized;
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(new Vector2(0f, 1f));
                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(edge.magnitude / tileSize, 0f));
                    uvs.Add(new Vector2(edge.magnitude / tileSize, 1f));

                    triangles.Add(sideStart);
                    triangles.Add(sideStart + 1);
                    triangles.Add(sideStart + 2);

                    triangles.Add(sideStart);
                    triangles.Add(sideStart + 2);
                    triangles.Add(sideStart + 3);
                }
            }
        }

        Mesh mesh = new Mesh();
        if (vertices.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();

        return mesh;
    }

    private Material CreateDefaultMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        Material material = new Material(shader);
        material.color = new Color(0.45f, 0.72f, 0.34f);
        return material;
    }
}