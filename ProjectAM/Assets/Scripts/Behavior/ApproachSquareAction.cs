using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 광장 칸 중 갈 수 있으면서 가장 가까운 자리까지 이동한다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ApproachSquare", story: "[Agent] Approach Square", category: "Action", id: "a4d9e6f1b3c5697a0d2e4f6a8b0c3d5e")]
public partial class ApproachSquareAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;

    private const float MaxWaitTime = 3.0f;

    private Movement movement;
    private Vector2Int destTile;

    private float waitTime = 0.0f;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        movement = npc.Movement;
        if (movement == null) return Status.Failure;

        Square square = World.Instance.Square;
        if (square == null) return Status.Failure;

        destTile = SelectApproachTile(square, npc.CurrentTile);
        if (destTile == npc.CurrentTile) return Status.Failure;

        movement.StartMoveTo(destTile);
        if (movement.State == MoveState.Idle) return Status.Failure;

        waitTime = 0.0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (movement.State == MoveState.Moving)
        {
            waitTime = 0.0f;
            return Status.Running;
        }
        if (movement.State == MoveState.Waiting)
        {
            waitTime += Time.deltaTime;
            if (waitTime >= MaxWaitTime)
            {
                return Status.Failure;
            }
            else
            {
                return Status.Running;
            }
        }

        return (movement.CurrentTile == destTile) ? Status.Success : Status.Failure;
    }

    protected override void OnEnd()
    {
        Agent.Value?.Movement.StopMoving();
    }

    // 갈 수 있는 광장 칸 중 가장 가까운 것. 없으면 출발 타일을 그대로 돌려준다
    private Vector2Int SelectApproachTile(Square square, Vector2Int from)
    {
        Vector2Int nearest = from;
        int nearestDist = int.MaxValue;

        foreach (Vector2Int tile in square.GetApproachTiles())
        {
            if (!movement.IsMovable(tile)) continue;

            int dist = TileCoordinate.CalcManhattanDist(from, tile);
            if (dist >= nearestDist) continue;

            nearest = tile;
            nearestDist = dist;
        }

        return nearest;
    }
}
