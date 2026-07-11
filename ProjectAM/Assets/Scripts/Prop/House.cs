using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private const string RoofObjectName = "Cabin_Roof";

    public NpcColor owner;
    public Vector2Int anchor;
    public Vector2Int origin;
    public Vector2Int size;
    public Vector2Int entrance;

    public List<NpcColor> guests;

    // 방문객이 서는 마당('Y') 타일 하나를 무작위로 고른다.
    public Vector2Int FindRandomYardTile(MapData mapData)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        int width = mapData.resolution.x;
        int height = mapData.resolution.y;

        for (int row = anchor.y; row < anchor.y + size.y; row++)
        {
            for (int col = anchor.x; col < anchor.x + size.x; col++)
            {
                if (row < 0 || col < 0 || row >= height || col >= width) continue;
                if (mapData.fieldTypes[row, col] != 'Y') continue;

                candidates.Add(new Vector2Int(col, row));
            }
        }

        if (candidates.Count == 0) return entrance;

        return candidates[Random.Range(0, candidates.Count)];
    }

    public void ApplyRoofColor(NpcColor color)
    {
        Material colorMaterial = Resources.Load<Material>($"Materials/{color}");
        if (colorMaterial == null)
        {
            Debug.LogWarning($"Roof color material not found : {color}");
            return;
        }

        Renderer roofRenderer = null;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name != RoofObjectName) continue;

            roofRenderer = child.GetComponent<Renderer>();
            break;
        }

        if (roofRenderer == null)
        {
            Debug.LogWarning($"Roof object not found : {RoofObjectName}");
            return;
        }

        roofRenderer.sharedMaterial = colorMaterial;
    }
}
