using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WalkAround", story: "[Agent] Move To Random Tile", category: "Action", id: "a9c968d44a5cc48d7a3324bb740fa600")]
public partial class WalkAroundAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    private const int DestRadius = 8;
    private const float MaxWaitTime = 3.0f;

    private Movement movement;
    private Vector2Int destTile;
    private readonly List<Vector2Int> candidates = new List<Vector2Int>();
    private float waitTime = 0.0f;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        movement = npc.Movement;
        if (movement == null) return Status.Failure;

        MapData mapData = World.Instance.MapData;
        if (mapData == null) return Status.Failure;

        destTile = SelectDestination(npc.CurrentTile, mapData);
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
                movement.Finish();
                return Status.Failure;
            }
            else
            {
                return Status.Running;                
            }
        }

        return (movement.CurrentTile == destTile) ? Status.Success : Status.Failure;
    }

    protected override void OnEnd() { }

    private Vector2Int SelectDestination(Vector2Int origin, MapData mapData)
    {
        candidates.Clear();
        
        int width = mapData.resolution.x;
        int height = mapData.resolution.y;

        for (int row = origin.y - DestRadius; row <= origin.y + DestRadius; row++)
        {
            for (int col = origin.x - DestRadius; col <= origin.x + DestRadius; col++)
            {
                if (row < 0 || col < 0 || row >= height || col >= width) continue;
                if (Mathf.Abs(row - origin.y) + Mathf.Abs(col - origin.x) > DestRadius) continue;
                if (!Movement.MovableTypes.Contains(mapData.fieldTypes[row, col])) continue;
                
                Vector2Int tile = new Vector2Int(col, row);
                
                if (!World.Instance.MapRuntime.IsEmpty(tile)) continue;
                if (tile == origin) continue;

                candidates.Add(tile);
            }
        }

        if (candidates.Count == 0)
        {
            return origin;
        }
        else
        {
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
    }
}
