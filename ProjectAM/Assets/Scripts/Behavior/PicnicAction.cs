using System;
using System.Collections.Generic;
using DG.Tweening;
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
    private const float StandUpDuration = 1.2f;
    private const float FaceTurnDuration = 0.4f;

    private static readonly Vector2Int[] NeighborOffsets =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)
    };

    private enum Phase { Move, Stay, StandUp }

    private NPC npc;
    private Movement movement;
    private NpcAnimation animation;
    private Vector2Int destTile;
    private Phase phase;
    private float waitTime;
    private float picnicDuration;
    private float picnicElapsed;
    private float standUpElapsed;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        animation = npc.GetComponent<NpcAnimation>();
        if (animation == null) return Status.Failure;

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
        if (phase == Phase.Stay) return UpdateStay();

        return UpdateStandUp();
    }

    protected override void OnEnd()
    {
        // 도중에 끊겨도 앉은 자세로 남지 않게 한다
        if (animation != null) animation.SetSitting(false);
    }

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

        // 도착: 바다를 바라보고 앉는다
        FaceWater();
        animation.SetSitting(true);

        phase = Phase.Stay;
        picnicElapsed = 0.0f;
        picnicDuration = UnityEngine.Random.Range(MinPicnicDuration, MaxPicnicDuration);

        return Status.Running;
    }

    private Status UpdateStay()
    {
        picnicElapsed += Time.deltaTime;
        if (picnicElapsed < picnicDuration) return Status.Running;

        // 소풍 끝: 일어나는 동작이 끝날 때까지 기다렸다가 마친다
        animation.SetSitting(false);
        phase = Phase.StandUp;
        standUpElapsed = 0.0f;

        return Status.Running;
    }

    private Status UpdateStandUp()
    {
        standUpElapsed += Time.deltaTime;
        return (standUpElapsed >= StandUpDuration) ? Status.Success : Status.Running;
    }

    // 목적지에 맞닿은 물 타일 방향으로 몸을 돌린다
    private void FaceWater()
    {
        MapData mapData = World.Instance.MapData;

        foreach (Vector2Int offset in NeighborOffsets)
        {
            Vector2Int neighbor = destTile + offset;

            bool isOutOfBounds = neighbor.x < 0 || neighbor.x >= mapData.resolution.x
                || neighbor.y < 0 || neighbor.y >= mapData.resolution.y;
            if (!isOutOfBounds && mapData.fieldTypes[neighbor.y, neighbor.x] != ' ') continue;

            Vector3 direction = new Vector3(offset.x, 0f, -offset.y);
            npc.transform.DORotateQuaternion(Quaternion.LookRotation(direction), FaceTurnDuration);
            return;
        }
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
