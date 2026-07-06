using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MapGridDebugRenderer : MonoBehaviour
{
    [SerializeField]
    private float tileSize = 1.0f;
    [SerializeField]
    private float height = 1.0f;
    [SerializeField]
    private Color gridColor = new Color(1f, 1f, 1f, 0.01f);
    [SerializeField]
    private Color fieldColor = new Color(0.2f, 1f, 0.3f, 0.8f);
    [SerializeField]
    private Color bridgeColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField]
    private Color houseColor = new Color(0.2f, 0.4f, 1f, 0.8f);
    [SerializeField]
    private Color squareColor = new Color(0.9f, 0.9f, 0.7f, 0.8f);

    private MapData mapData;

    public void BindMapData(MapData mapData)
    {
        this.mapData = mapData;
    }

    private void OnDrawGizmos()
    {
        if (mapData == null) return;

        int height = mapData.resolution.y;
        int width = mapData.resolution.x;

        Gizmos.color = gridColor;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                DrawCellBorder(row, col);
            }
        }

        Gizmos.color = fieldColor;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if(mapData.fieldTypes[row, col] == 'A') DrawCellBorder(row, col);
            }
        }

        Gizmos.color = bridgeColor;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if(mapData.fieldTypes[row, col] == 'B') DrawCellBorder(row, col);
            }
        }

        Gizmos.color = houseColor;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if(mapData.fieldTypes[row, col] == 'H' || mapData.fieldTypes[row, col] == 'Y') DrawCellBorder(row, col);
            }
        }

        Gizmos.color = squareColor;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if(mapData.fieldTypes[row, col] == 'S') DrawCellBorder(row, col);
            }
        }
    }

    private void DrawCellBorder(int row, int col)
    {
        Vector3 topLeft = GridToWorld(col - 0.5f, -(row - 0.5f));
        Vector3 topRight = GridToWorld(col + 0.5f, -(row - 0.5f));
        Vector3 bottomRight = GridToWorld(col + 0.5f, -(row + 0.5f));
        Vector3 bottomLeft = GridToWorld(col - 0.5f, -(row + 0.5f));

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    private Vector3 GridToWorld(float x, float y)
    {
        return transform.TransformPoint(new Vector3(x * tileSize, height, y * tileSize));
    }
}
