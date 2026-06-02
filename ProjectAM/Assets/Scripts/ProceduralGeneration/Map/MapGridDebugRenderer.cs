using UnityEngine;

// 원본 배열을 타일 테두리로 시각화하는 디버그용 컴포넌트.
// MarchingSquaresTerrain과 같은 GameObject에 붙여 좌표를 메쉬와 맞춘다.
// 전체 격자(gridColor) 위에 solid 셀(solidColor)을 덧그린다.
public class MapGridDebugRenderer : MonoBehaviour
{
    [SerializeField]
    private float threshold = 0.5f;
    [SerializeField]
    private float tileSize = 1.0f;
    [SerializeField]
    private float padding = 1.0f;
    [SerializeField]
    private float height = 1.0f;
    [SerializeField]
    private Color gridColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField]
    private Color solidColor = new Color(0.2f, 1f, 0.3f, 0.8f);

    private float[,] map;

    public void BindScalaField(float[,] map)
    {
        this.map = map;
    }

    private void OnDrawGizmos()
    {
        if (map == null) return;

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        Gizmos.color = gridColor;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                DrawCellBorder(row, col);
            }
        }

        Gizmos.color = solidColor;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (map[row, col] > threshold) DrawCellBorder(row, col);
            }
        }
    }

    private void DrawCellBorder(int row, int col)
    {
        Vector3 topLeft = CornerToWorld(row, col);
        Vector3 topRight = CornerToWorld(row, col + 1);
        Vector3 bottomRight = CornerToWorld(row + 1, col + 1);
        Vector3 bottomLeft = CornerToWorld(row + 1, col);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    // MarchingSquares.FieldPoint와 동일한 인덱스 → 위치 매핑.
    private Vector3 CornerToWorld(int row, int col)
    {
        float x = (col - padding) * tileSize;
        float z = -(row - padding) * tileSize;
        return transform.TransformPoint(new Vector3(x, height, z));
    }
}
