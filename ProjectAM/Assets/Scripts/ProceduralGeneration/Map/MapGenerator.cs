using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MapGenerator
{
    [SerializeField]
    private float tileSize = 2f;
    [SerializeField]
    private float topHeight = 1.0f;
    [SerializeField]
    private float bottomHeight = 0.0f;

    public List<Vector2> ExtractOutline(MapData mapData, float tileSize = 1f)
    {
        List<Vector2Int> rawLoop = new List<Vector2Int>();
        Stack<Vector2Int> dfsStack = new Stack<Vector2Int>();
        bool[,] isVisited = new bool[mapData.resolution.y, mapData.resolution.x];

        for (int row = 0; row < mapData.resolution.y; row++)
        {
            for(int col = 0; col < mapData.resolution.x; col++)
            {
                if (mapData.map[row, col] == 'O')
                {
                    Vector2Int start = new Vector2Int(col, row);
                    rawLoop.Add(start);
                    isVisited[row, col] = true;

                    dfsStack.Push(start);
                    break;
                }
            }

            if (dfsStack.Count > 0) break;
        }

        while(dfsStack.Count > 0)
        {
            Vector2Int curPos = dfsStack.Pop();

            int[] dr = new int[8] { -1, 1, 0, 0, -1, -1, 1, 1 };
            int[] dc = new int[8] { 0, 0, -1, 1, -1, 1, -1, 1 };

            for (int i = 0; i < 8; i++)
            {
                Vector2Int nextPos = curPos + new Vector2Int(dc[i], dr[i]);

                if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= mapData.resolution.x || nextPos.y >= mapData.resolution.y) continue;
                if (isVisited[nextPos.y, nextPos.x]) continue;
                if (mapData.map[nextPos.y, nextPos.x] != 'O') continue;

                rawLoop.Add(nextPos);
                isVisited[nextPos.y, nextPos.x] = true;

                dfsStack.Push(nextPos);
                break;
            }
        }


        List<Vector2> loop = new List<Vector2>(rawLoop.Count);
        
        for(int i = 0; i < rawLoop.Count; i++)
        {
            Vector2Int position = rawLoop[i];
            loop.Add(new Vector2(position.x * tileSize, position.y * tileSize));
        }

        return loop;
    }

    public Mesh BuildMesh(List<Vector2> polygon)
    {
        EnsureClockwise(polygon);

        List<Vector3> vertices = new List<Vector3>(polygon.Count * 6);
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>(polygon.Count * 6);
        List<Vector2> uvs = new List<Vector2>(polygon.Count * 6);

        // top surface
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 point = polygon[i];
            vertices.Add(new Vector3(point.x, topHeight, point.y));
            normals.Add(Vector3.up);
            uvs.Add(point / tileSize);
        }

        // bottom surface
        int bottomOffset = vertices.Count;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 point = polygon[i];
            vertices.Add(new Vector3(point.x, bottomHeight, point.y));
            normals.Add(Vector3.down);
            uvs.Add(point / tileSize);
        }

        // top triangles
        List<int> remaining = new List<int>(polygon.Count);
        for (int i = 0; i < polygon.Count; i++)
        {
            remaining.Add(i);
        }

        int maxIterations = polygon.Count * polygon.Count;
        while ((remaining.Count > 3) && (maxIterations-- > 0))
        {
            bool success = EarClipping.TryEarClipping(polygon, remaining, triangles);

            if (success == false)
            {
                Debug.LogWarning("Polygon triangulation stopped early. Check for self-intersections.");
                break;
            }
        }

        if (remaining.Count == 3)
        {
            triangles.Add(remaining[0]);
            triangles.Add(remaining[1]);
            triangles.Add(remaining[2]);
        }

        // bottom triangles
        int topTriangleCount = triangles.Count;
        for (int i = 0; i < topTriangleCount; i += 3)
        {
            triangles.Add(bottomOffset + triangles[i]);
            triangles.Add(bottomOffset + triangles[i + 2]);
            triangles.Add(bottomOffset + triangles[i + 1]);
        }

        // side faces
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % polygon.Count];
            Vector2 edge = next - current;
            Vector3 sideNormal = new Vector3(edge.y, 0f, -edge.x).normalized;

            int sideOffset = vertices.Count;

            vertices.Add(new Vector3(current.x, topHeight, current.y));
            vertices.Add(new Vector3(next.x, topHeight, next.y));
            vertices.Add(new Vector3(next.x, bottomHeight, next.y));
            vertices.Add(new Vector3(current.x, bottomHeight, current.y));

            normals.Add(sideNormal);
            normals.Add(sideNormal);
            normals.Add(sideNormal);
            normals.Add(sideNormal);

            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(edge.magnitude / tileSize, 1f));
            uvs.Add(new Vector2(edge.magnitude / tileSize, 0f));
            uvs.Add(new Vector2(0f, 0f));

            triangles.Add(sideOffset + 0);
            triangles.Add(sideOffset + 2);
            triangles.Add(sideOffset + 1);

            triangles.Add(sideOffset + 0);
            triangles.Add(sideOffset + 3);
            triangles.Add(sideOffset + 2);
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

    private static void EnsureClockwise(List<Vector2> polygon)
    {
        if (GetSignedArea(polygon) > 0f) polygon.Reverse();
    }

    private static float GetSignedArea(IReadOnlyList<Vector2> polygon)
    {
        float area = 0f;
        for (int index = 0; index < polygon.Count; index++)
        {
            Vector2 current = polygon[index];
            Vector2 next = polygon[(index + 1) % polygon.Count];
            area += current.x * next.y - next.x * current.y;
        }

        return area * 0.5f;
    }
}