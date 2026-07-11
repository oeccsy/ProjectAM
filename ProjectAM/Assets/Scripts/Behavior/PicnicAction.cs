using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Picnic", story: "[Agent] Go Picnic At Island Edge", category: "Action", id: "f2b8c4d6e0a3528b9c1d2e3f4a5b6c7e")]
public partial class PicnicAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    private const float MaxWaitTime = 3.0f;
    private const float MinPicnicDuration = 8.0f;
    private const float MaxPicnicDuration = 15.0f;

    private static readonly Vector2Int[] NeighborOffsets =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)
    };

    private enum Phase { Move, Stay }

    private NPC npc;
    private Movement movement;
    private Vector2Int destTile;
    private Phase phase;
    private float waitTime;
    private float picnicDuration;
    private float picnicElapsed;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        movement = npc.Movement;
        if (movement.State != MoveState.Idle) return Status.Failure;

        MapData mapData = World.Instance.MapData;
        List<Vector2Int> edgeTiles = FindIslandEdgeTiles(mapData);
        if (edgeTiles.Count == 0) return Status.Failure;

        destTile = edgeTiles[UnityEngine.Random.Range(0, edgeTiles.Count)];
        if (destTile == npc.CurrentTile) return Status.Failure;

        movement.StartMoveTo(destTile);
        if (movement.State == MoveState.Idle) return Status.Failure;

        phase = Phase.Move;
        waitTime = 0.0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (phase == Phase.Move) return UpdateMove();

        return UpdateStay();
    }

    protected override void OnEnd() { }

    private Status UpdateMove()
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
            return Status.Running;
        }

        if (npc.CurrentTile != destTile) return Status.Failure;

        phase = Phase.Stay;
        picnicElapsed = 0.0f;
        picnicDuration = UnityEngine.Random.Range(MinPicnicDuration, MaxPicnicDuration);

        return Status.Running;
    }

    private Status UpdateStay()
    {
        picnicElapsed += Time.deltaTime;
        return (picnicElapsed >= picnicDuration) ? Status.Success : Status.Running;
    }

    // 섬 외곽 = 일반 지면('A') 타일 중 물과 맞닿은 자리
    private List<Vector2Int> FindIslandEdgeTiles(MapData mapData)
    {
        List<Vector2Int> edgeTiles = new List<Vector2Int>();

        int height = mapData.resolution.y;
        int width = mapData.resolution.x;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (mapData.fieldTypes[row, col] != 'A') continue;

                Vector2Int tile = new Vector2Int(col, row);

                if (!IsNextToWater(tile, mapData)) continue;
                if (!World.Instance.MapRuntime.IsEmpty(tile)) continue;

                edgeTiles.Add(tile);
            }
        }

        return edgeTiles;
    }

    private bool IsNextToWater(Vector2Int tile, MapData mapData)
    {
        foreach (Vector2Int offset in NeighborOffsets)
        {
            Vector2Int neighbor = tile + offset;

            if (neighbor.x < 0 || neighbor.x >= mapData.resolution.x) return true;
            if (neighbor.y < 0 || neighbor.y >= mapData.resolution.y) return true;
            if (mapData.fieldTypes[neighbor.y, neighbor.x] == ' ') return true;   // 빈 타일 = 물
        }

        return false;
    }
}
