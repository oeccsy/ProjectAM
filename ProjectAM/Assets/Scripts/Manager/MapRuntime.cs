using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRuntime
{
    private IMovable[,] map;

    public bool IsEmpty(Vector2Int tile) => map[tile.y, tile.x] == null;
    public IMovable GetOccupant(Vector2Int tile) => map[tile.y, tile.x];

    public MapRuntime(Vector2Int resolution)
    {
        map = new IMovable[resolution.y, resolution.x];
    }

    public void Occupy(IMovable movable, Vector2Int tile)
    {
        map[tile.y, tile.x] = movable;
    }

    public bool TryOccupy(IMovable movable, Vector2Int tile)
    {
        IMovable occupant = map[tile.y, tile.x];
        if (occupant != null && occupant != movable) return false;

        map[tile.y, tile.x] = movable;
        return true;
    }

    public void Release(IMovable movable, Vector2Int tile)
    {
        if (map[tile.y, tile.x] == movable) map[tile.y, tile.x] = null;
    }
}
