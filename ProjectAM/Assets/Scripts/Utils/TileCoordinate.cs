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

    public static Vector3 TileToWorld(Vector2Int tile)
    {
        TerrainScaleSettings scale = World.Instance.TerrainScaleSettings;
        float x = tile.x * scale.tileSize;
        float z = -tile.y * scale.tileSize;
        return new Vector3(x, scale.topHeight, z);
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
