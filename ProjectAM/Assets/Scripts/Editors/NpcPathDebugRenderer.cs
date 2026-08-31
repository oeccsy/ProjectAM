#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC에 붙여두면, 씬/하이어라키에서 그 NPC를 선택했을 때만
/// 현재 A* 경로를 타일 그리드 + 연결선으로 그려준다 (디버그용).
/// </summary>
[RequireComponent(typeof(Movement))]
public class NpcPathDebugRenderer : MonoBehaviour
{
    [SerializeField] private Color tileColor = new Color(0f, 1f, 1f, 0.35f);
    [SerializeField] private Color destColor = Color.yellow;
    [SerializeField] private Color lineColor = Color.cyan;
    [SerializeField] private float debugTileScale = 0.85f;
    [SerializeField] private float heightOffset = 0.05f;

    private Movement movement;
    private Movement Movement
    {
        get
        {
            if (movement == null) movement = GetComponent<Movement>();
            return movement;
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawPath();
    }

    private void DrawPath()
    {
        if (Movement == null) return;
        if (Movement.Path == null) return;
        if (Movement.Path.Count <= 1) return;
        if (Movement.State == MoveState.Idle) return;

        List<Vector2Int> path = Movement.Path;

        TerrainScaleSettings scale = World.Instance.TerrainScaleSettings;

        Vector3 cubeSize = new Vector3(scale.tileSize * debugTileScale, 0.05f, scale.tileSize * debugTileScale);
        Vector3 prevTilePos = Vector3.zero;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 curTilePos = TileCoordinate.TileToWorld(path[i], scale);
            curTilePos.y += heightOffset;

            Gizmos.color = (i == path.Count - 1) ? destColor : tileColor;
            Gizmos.DrawCube(curTilePos, cubeSize);

            if (i > 0)
            {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(prevTilePos, curTilePos);
            }

            prevTilePos = curTilePos;
        }
    }
}

#endif