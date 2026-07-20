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

    private NPC npc;
    private Vector2Int destTile;
    private readonly List<Vector2Int> candidates = new List<Vector2Int>();
    private float waitTime = 0.0f;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        destTile = SelectDestination(npc.CurrentTile);
        if (destTile == npc.CurrentTile) return Status.Failure;

        npc.Movement.StartMoveTo(destTile);
        waitTime = 0.0f;
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (npc.Movement.State == MoveState.Moving)
        {
            waitTime = 0.0f;
            return Status.Running;
        }
        if (npc.Movement.State == MoveState.Waiting)
        {
            waitTime += Time.deltaTime;
            if (waitTime >= MaxWaitTime)
            {
                npc.Movement.StopMoving();
                return Status.Failure;
            }
            else
            {
                return Status.Running;                
            }
        }

        return (npc.Movement.CurrentTile == destTile) ? Status.Success : Status.Failure;
    }

    protected override void OnEnd() { }

    private Vector2Int SelectDestination(Vector2Int origin)
    {
        candidates.Clear();
        
        MapData mapData = World.Instance.MapData;
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
