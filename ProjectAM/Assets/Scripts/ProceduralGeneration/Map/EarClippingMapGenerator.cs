using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class EarClippingMapGenerator : MonoBehaviour
{
    private struct Segment
    {
        public Vector2 pointA;
        public Vector2 pointB;
        public Vector2Int idA;
        public Vector2Int idB;

        public Segment(Vector2 pointA, Vector2 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            
            idA = new Vector2Int()
            {
                x = Mathf.RoundToInt(pointA.x * 1000f),
                y = Mathf.RoundToInt(pointA.y * 1000f)
            };
            
            idB = new Vector2Int()
            {
                x = Mathf.RoundToInt(pointB.x * 1000f),
                y = Mathf.RoundToInt(pointB.y * 1000f)
            };
        }
    }

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

    public void Build(MapData mapData)
    {
        float[,] field = mapData.value;
        List<Segment> segments = CreateMarchingSegments(field, 0.5f);
        List<Vector2> contour = ConnectLargestLoop(segments);

        if (contour.Count < 3)
        {
            Debug.LogWarning("No valid marching-squares contour was found.");
            return;
        }

        Mesh mesh = BuildMesh(contour);
        mesh.name = "Marching Squares Terrain";
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        Debug.Log($"Marching Squares EarClipping Terrain: segments={segments.Count}, contour={contour.Count}");
    }

    private List<Segment> CreateMarchingSegments(float[,] field, float threshold = 0.5f)
    {
        List<Segment> segments = new List<Segment>();
        int rows = field.GetLength(0);
        int cols = field.GetLength(1);

        for (int row = 0; row < rows - 1; row++)
        {
            for (int col = 0; col < cols - 1; col++)
            {
                float topLeftValue = field[row, col];
                float topRightValue = field[row, col + 1];
                float bottomRightValue = field[row + 1, col + 1];
                float bottomLeftValue = field[row + 1, col];

                int mask = 0;
                if (topLeftValue > threshold) mask |= 1;
                if (topRightValue > threshold) mask |= 2;
                if (bottomRightValue > threshold) mask |= 4;
                if (bottomLeftValue > threshold) mask |= 8;
                if (mask == 0 || mask == 15) continue;

                Vector2 topLeft = new Vector2(col, row);
                Vector2 topRight = new Vector2(col + 1, row);
                Vector2 bottomRight = new Vector2(col + 1, row + 1);
                Vector2 bottomLeft = new Vector2(col, row + 1);

                Vector2 top = Interpolate(topLeft, topRight, topLeftValue, topRightValue);
                Vector2 right = Interpolate(topRight, bottomRight, topRightValue, bottomRightValue);
                Vector2 bottom = Interpolate(bottomLeft, bottomRight, bottomLeftValue, bottomRightValue);
                Vector2 left = Interpolate(topLeft, bottomLeft, topLeftValue, bottomLeftValue);

                switch (mask)
                {
                    case 0:
                        break;
                    case 1:
                        AddSegment(left, top, segments);
                        break;
                    case 2: 
                        AddSegment(top, right, segments);
                        break;
                    case 3: 
                        AddSegment(left, right, segments);
                        break;
                    case 4: 
                        AddSegment(right, bottom, segments);
                        break;
                    case 5:
                        Debug.Log($"ambiguous case : case 5");
                        AddSegment(left, top, segments);
                        AddSegment(right, bottom, segments);
                        break;
                    case 6: 
                        AddSegment(top, bottom, segments);
                        break;
                    case 7: 
                        AddSegment(left, bottom, segments);
                        break;
                    case 8: 
                        AddSegment(bottom, left, segments);
                        break;
                    case 9: 
                        AddSegment(top, bottom, segments);
                        break;
                    case 10:
                        Debug.Log($"ambiguous case : case 10");
                        AddSegment(top, right, segments);
                        AddSegment(bottom, left, segments);
                        break;
                    case 11:
                        AddSegment(right, bottom, segments);
                        break;
                    case 12:
                        AddSegment(right, left, segments);
                        break;
                    case 13:
                        AddSegment(top, right, segments);
                        break;
                    case 14:
                        AddSegment(left, top, segments);
                        break;
                    case 15:
                        break;
                }
            }
        }

        return segments;
    }
    
    private static Vector2 Interpolate(Vector2 a, Vector2 b, float valueA, float valueB)
    {
        float range = valueB - valueA;
        if (Mathf.Abs(range) < 0.0001f) return (a + b) * 0.5f;

        float t = Mathf.Clamp01((0.5f - valueA) / range);
        return Vector2.Lerp(a, b, t);
    }

    private void AddSegment(Vector2 a, Vector2 b, List<Segment> segments)
    {
        if ((a - b).sqrMagnitude > 0.000001f) segments.Add(new Segment(a, b));
    }

    private List<Vector2> ConnectLargestLoop(IReadOnlyList<Segment> segments)
    {
        List<Vector2> largestLoop = new List<Vector2>();
        bool[] isUsed = new bool[segments.Count];

        for (int i = 0; i < segments.Count; i++)
        {
            if (isUsed[i]) continue;

            List<Vector2> tempLoop = ConnectLoop(segments, isUsed, i);
            if (tempLoop.Count >= 3 && tempLoop.Count > largestLoop.Count)
            {
                largestLoop = tempLoop;
            }
        }

        return largestLoop;
    }

    private List<Vector2> ConnectLoop(IReadOnlyList<Segment> segments, bool[] used, int startIndex)
    {
        List<Vector2> loop = new List<Vector2>();
        
        Segment startSegment = segments[startIndex];
        Vector2Int startID = startSegment.idA;
        
        loop.Add(startSegment.pointA);

        Segment currentSegment = startSegment;
        Vector2Int currentID = startSegment.idB;
        used[startIndex] = true;

        int safety = segments.Count + 1;
        while (safety-- > 0)
        {
            if (currentID == startID) break;

            Vector2 currentPoint = (currentID == currentSegment.idA) ? currentSegment.pointA : currentSegment.pointB;
            loop.Add(currentPoint);

            int nextIndex = -1;
            for (int i = 0; i < segments.Count; i++)
            {
                if (used[i]) continue;
                if (segments[i].idA == currentID || segments[i].idB == currentID)
                {
                    nextIndex = i;
                    break;
                }
            }

            if (nextIndex < 0) break;

            currentSegment = segments[nextIndex];
            currentID = (currentID == currentSegment.idA) ? currentSegment.idB : currentSegment.idA;
            used[nextIndex] = true;
        }

        return loop;
    }

    private Mesh BuildMesh(List<Vector2> polygon)
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

    private static Material CreateDefaultMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.color = new Color(0.45f, 0.72f, 0.34f);
        return material;
    }
}