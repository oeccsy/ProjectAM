using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 본인 집을 뺀 남의 집 하나를 무작위로 골라 그 앞까지 이동한다.
/// 고른 집은 TargetHouse에 남겨 다음 노드가 이어받는다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ApproachOtherHouse", story: "[Agent] Approach Other [House]", category: "Action", id: "e2b7c4d9f1a3475e8b0c2d4e6f8a1b3c")]
public partial class ApproachOtherHouseAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;
    [SerializeReference] public BlackboardVariable<House> House;

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

        House house = SelectOtherHouse(npc.OwnColor);
        if (house == null) return Status.Failure;

        destTile = SelectEntranceTile(house, npc.CurrentTile);
        if (destTile == npc.CurrentTile) return Status.Failure;

        movement.StartMoveTo(destTile);
        if (movement.State == MoveState.Idle) return Status.Failure;

        if (House != null) House.Value = house;
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

    private House SelectOtherHouse(NpcColor ownColor)
    {
        List<House> candidates = new List<House>();

        foreach (House house in World.Instance.Houses.All)
        {
            if (house.owner == ownColor) continue;

            candidates.Add(house);
        }

        if (candidates.Count == 0) return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    // 마당 타일 중 방문자에게 가장 가까우면서 설 수 있는 칸. 없으면 출발 타일을 그대로 돌려준다
    private Vector2Int SelectEntranceTile(House house, Vector2Int from)
    {
        Vector2Int nearest = from;
        int nearestDist = int.MaxValue;

        foreach (Vector2Int tile in house.GetEntranceTiles())
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
