using UnityEngine;

public class MapRuntime
{
    private IMovable[,] map;

    public bool IsEmpty(Vector2Int tile) => map[tile.y, tile.x] == null;
    public void Reserve(IMovable movable, Vector2Int tile) => map[tile.y, tile.x] = movable;
    public void Release(IMovable movable, Vector2Int tile) => map[tile.y, tile.x] = (map[tile.y, tile.x] == movable) ? null : map[tile.y, tile.x];
    public IMovable GetReserver(Vector2Int tile) => map[tile.y, tile.x];

    public MapRuntime(Vector2Int resolution)
    {
        map = new IMovable[resolution.y, resolution.x];
    }
}
