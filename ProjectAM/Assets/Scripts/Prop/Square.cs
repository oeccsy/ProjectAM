using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public string assetType;
    public Vector2Int anchor;
    public Vector2 origin;
    public Vector2Int size;

    public List<Vector2Int> GetApproachTiles()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int row = anchor.y; row < anchor.y + size.y; row++)
        {
            for (int col = anchor.x; col < anchor.x + size.x; col++)
            {
                Vector2Int tile = new Vector2Int(col, row);
                if (World.Instance.MapRuntime.IsEmpty(tile)) tiles.Add(tile);
            }
        }

        return tiles;
    }
}
