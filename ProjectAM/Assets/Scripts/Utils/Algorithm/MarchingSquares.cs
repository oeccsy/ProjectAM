using System.Collections.Generic;
using UnityEngine;

public class MarchingSquares
{
    public readonly struct CellData
    {
        public readonly Vector2[] points;
        public readonly int[] contours;
        public readonly int[] triangles;

        public CellData(Vector2[] points, int[] contours, int[] triangles)
        {
            this.points = points;
            this.contours = contours;
            this.triangles = triangles;
        }
    }

    /*
    *    0        1
    *    ┌─────────┐
    *    │    4    │        0~3 : 모서리
    *    │7       5│        4~7 : 변
    *    │    6    │        
    *    └─────────┘        
    *    3        2
    */

    public const int TopLeft = 0;
    public const int TopRight = 1;
    public const int BottomRight = 2;
    public const int BottomLeft = 3;
    public const int Top = 4;
    public const int Right = 5;
    public const int Bottom = 6;
    public const int Left = 7;

    public static readonly int[][] EdgeTable = new int[][]
    {
        new int[] {},
        new int[] { Left, Bottom },
        new int[] { Bottom, Right },
        new int[] { Left, Right },
        new int[] { Right, Top },
        new int[] { Left, Bottom, Right, Top },
        new int[] { Bottom, Top },
        new int[] { Left, Top },
        new int[] { Top, Left },
        new int[] { Top, Bottom },
        new int[] { Top, Left, Bottom, Right },
        new int[] { Top, Right },
        new int[] { Right, Left },
        new int[] { Right, Bottom },
        new int[] { Bottom, Left },
        new int[] {}
    };

    public static readonly int[][] PolygonTable = new int[][]
    {
        new int[] {},
        new int[] { BottomLeft, Left, Bottom },
        new int[] { BottomRight, Bottom, Right },
        new int[] { Left, Right, BottomRight, Left, BottomRight, BottomLeft },
        new int[] { TopRight, Right, Top },
        new int[] { TopRight, Right, Top, BottomLeft, Left, Bottom },
        new int[] { TopRight, BottomRight, Bottom, TopRight, Bottom, Top },
        new int[] { Top, TopRight, BottomRight, Top, BottomRight, BottomLeft, Top, BottomLeft, Left },
        new int[] { TopLeft, Top, Left },
        new int[] { TopLeft, Top, Bottom, TopLeft, Bottom, BottomLeft },
        new int[] { TopLeft, Top, Left, BottomRight, Bottom, Right },
        new int[] { TopLeft, Top, Right, TopLeft, Right, BottomRight, TopLeft, BottomRight, BottomLeft },
        new int[] { TopLeft, TopRight, Right, TopLeft, Right, Left },
        new int[] { TopLeft, TopRight, Right, TopLeft, Right, Bottom, TopLeft, Bottom, BottomLeft },
        new int[] { TopLeft, TopRight, BottomRight, TopLeft, BottomRight, Bottom, TopLeft, Bottom, Left },
        new int[] { TopLeft, TopRight, BottomRight, TopLeft, BottomRight, BottomLeft }
    };

    public static readonly Dictionary<int, int[]> AmbiguousCaseConnectedEdgeTable = new Dictionary<int, int[]>
    {
        { 5, new int[] { Left, Top, Right, Bottom } },
        { 10, new int[] { Top, Right, Bottom, Left } }
    };

    public static readonly Dictionary<int, int[]> AmbiguousCaseConnectedTriangleTable = new Dictionary<int, int[]>
    {
        { 5, new int[] { TopRight, Left, Top, TopRight, BottomLeft, Left, TopRight, Bottom, BottomLeft, TopRight, Right, Bottom } },
        { 10, new int[] { TopLeft, Bottom, Left, TopLeft, BottomRight, Bottom, TopLeft, Right, BottomRight, TopLeft, Top, Right } }
    };

    public CellData Sample(float[,] field, int row, int col, float threshold = 0.5f)
    {
        float topLeftValue = field[row, col];
        float topRightValue = field[row, col + 1];
        float bottomRightValue = field[row + 1, col + 1];
        float bottomLeftValue = field[row + 1, col];

        Vector2 topLeft = FieldPoint(row, col);
        Vector2 topRight = FieldPoint(row, col + 1);
        Vector2 bottomRight = FieldPoint(row + 1, col + 1);
        Vector2 bottomLeft = FieldPoint(row + 1, col);

        Vector2 top = Interpolate(topLeft, topRight, topLeftValue, topRightValue);
        Vector2 right = Interpolate(topRight, bottomRight, topRightValue, bottomRightValue);
        Vector2 bottom = Interpolate(bottomLeft, bottomRight, bottomLeftValue, bottomRightValue);
        Vector2 left = Interpolate(topLeft, bottomLeft, topLeftValue, bottomLeftValue);

        Vector2[] points = new Vector2[8];
        points[TopLeft] = topLeft;
        points[TopRight] = topRight;
        points[BottomRight] = bottomRight;
        points[BottomLeft] = bottomLeft;
        points[Top] = top;
        points[Right] = right;
        points[Bottom] = bottom;
        points[Left] = left;

        int mask = 0;
        if (topLeftValue > threshold) mask |= 8;
        if (topRightValue > threshold) mask |= 4;
        if (bottomRightValue > threshold) mask |= 2;
        if (bottomLeftValue > threshold) mask |= 1;

        int[] contours = EdgeTable[mask];
        int[] triangles = PolygonTable[mask];

        if ((mask == 5) || (mask == 10))
        {
            float centerValue = (topLeftValue + topRightValue + bottomRightValue + bottomLeftValue) * 0.25f;
            if(centerValue > threshold)
            {
                contours = AmbiguousCaseConnectedEdgeTable[mask];
                triangles = AmbiguousCaseConnectedTriangleTable[mask];
            }
        }

        CellData cellData = new CellData(points, contours, triangles);
        return cellData;
    }

    private Vector2 FieldPoint(int row, int col, int samplesPerTile = 1, float tileSize = 1.0f, float padding = 1.0f)
    {
        float mapX = col / (float)samplesPerTile - padding;
        float mapY = row / (float)samplesPerTile - padding;

        return new Vector2(mapX * tileSize, -mapY * tileSize);
    }

    private Vector2 Interpolate(Vector2 p1, Vector2 p2, float v1, float v2, float threshold = 0.5f)
    {
        if(Mathf.Abs(v2 - v1) < 0.0001f) return (p1 + p2) * 0.5f;

        float t = (threshold - v1) / (v2 - v1);
        return Vector2.Lerp(p1, p2, t);
    }
}