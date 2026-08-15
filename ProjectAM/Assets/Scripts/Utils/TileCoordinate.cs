using UnityEngine;

public static class TileCoordinate
{
    public static Vector2Int WorldToTile(Vector3 worldPos)
    {
        float tileSize = World.Instance.TerrainScaleSettings.tileSize;
        int col = Mathf.RoundToInt(worldPos.x / tileSize);
        int row = Mathf.RoundToInt(-worldPos.z / tileSize);
        return new Vector2Int(col, row);
    }

    public static Vector3 TileToWorld(Vector2 tile)
    {
        TerrainScaleSettings scale = World.Instance.TerrainScaleSettings;
        float x = tile.x * scale.tileSize;
        float z = -tile.y * scale.tileSize;
        return new Vector3(x, scale.topHeight, z);
    }

    public static Vector2 CalcCenterPos(Vector2Int anchor, Vector2Int size)
    {
        // 타일 인덱스 i는 그 타일의 중심을 가리키므로, size칸 블록의 중심은 (size - 1) / 2만큼 떨어져 있다.
        // 짝수 크기면 타일 경계 위에 놓이므로 정수로 떨어지지 않는다.
        return anchor + (size - Vector2.one) * 0.5f;
    }

    public static int CalcManhattanDist(Vector2Int src, Vector2Int dest)
    {
        int dx = Mathf.Abs(src.x - dest.x);
        int dy = Mathf.Abs(src.y - dest.y);

        return dx + dy;
    }

    public static int CalcChebyshevDist(Vector2Int src, Vector2Int dest)
    {
        int dx = Mathf.Abs(src.x - dest.x);
        int dy = Mathf.Abs(src.y - dest.y);

        return Mathf.Max(dx, dy);
    }
}
