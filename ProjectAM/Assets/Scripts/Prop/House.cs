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

    // 집 범위의 테두리 한 줄이 마당이다. 방문자는 이 중 한 칸에 선다
    public List<Vector2Int> GetEntranceTiles()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int row = anchor.y; row < anchor.y + size.y; row++)
        {
            for (int col = anchor.x; col < anchor.x + size.x; col++)
            {
                bool isSide = row == anchor.y || row == anchor.y + size.y - 1 || col == anchor.x || col == anchor.x + size.x - 1;
                if (!isSide) continue;

                tiles.Add(new Vector2Int(col, row));
            }
        }

        return tiles;
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
