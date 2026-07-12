using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Interact", story: "[Agent] Interact With Visible Npc", category: "Action", id: "e1a7b3c5d9f2417a8b0c1d2e3f4a5b6d")]
public partial class InteractAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    private const float MaxWaitTime = 3.0f;
    private const float MinTalkDuration = 2.0f;
    private const float MaxTalkDuration = 4.0f;

    private static readonly Vector2Int[] AdjacentOffsets =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)
    };

    private enum Phase { Approach, Talk }

    private NPC npc;
    private NPC target;
    private Movement movement;
    private Phase phase;
    private float waitTime;
    private float talkDuration;
    private float talkElapsed;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;
        if (!npc.Interaction.CanBeEngaged) return Status.Failure;

        movement = npc.Movement;
        if (movement.State != MoveState.Idle) return Status.Failure;

        target = FindNearestTarget();
        if (target == null) return Status.Failure;

        npc.Interaction.BeginEngage(target);
        target.Interaction.HoldBy(npc);

        phase = Phase.Approach;
        waitTime = 0.0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!target.IsAlive) return Status.Failure;   // 사건으로 상대가 사라진 경우

        if (phase == Phase.Approach) return UpdateApproach();

        return UpdateTalk();
    }

    protected override void OnEnd()
    {
        if (target != null) target.Interaction.Release();
        if (npc != null) npc.Interaction.Release();
    }

    private Status UpdateApproach()
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

        // Idle: 대상 옆에 도착했으면 대화 시작, 아니면 (다시) 접근
        if (IsAdjacentToTarget())
        {
            BeginTalk();
            return Status.Running;
        }

        Vector2Int destTile = SelectApproachTile();
        if (destTile == npc.CurrentTile) return Status.Failure;

        movement.StartMoveTo(destTile);
        if (movement.State == MoveState.Idle) return Status.Failure;

        return Status.Running;
    }

    private Status UpdateTalk()
    {
        talkElapsed += Time.deltaTime;
        if (talkElapsed < talkDuration) return Status.Running;

        int day = World.Instance.Time.Day;
        int hour = (int)World.Instance.Time.Hour;

        ContactClue contact = new ContactClue(day, hour, npc.OwnColor, target.OwnColor);

        World.Instance.Contacts.Record(contact);
        npc.Memory.Remember(contact);
        target.Memory.Remember(contact);

        // 의심/소문 전파
        npc.Memory.ShareRandomClue(target.Memory);
        target.Memory.ShareRandomClue(npc.Memory);

        return Status.Success;
    }

    private void BeginTalk()
    {
        phase = Phase.Talk;
        talkElapsed = 0.0f;
        talkDuration = UnityEngine.Random.Range(MinTalkDuration, MaxTalkDuration);

        npc.Interaction.BeginTalkWith(target);
        target.Interaction.BeginTalkWith(npc);
    }

    private NPC FindNearestTarget()
    {
        NPC nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (NPC candidate in npc.Perception.FindVisibleNpcs())
        {
            if (!candidate.Interaction.CanBeEngaged) continue;

            float sqrDistance = (candidate.transform.position - npc.transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearest = candidate;
                nearestSqrDistance = sqrDistance;
            }
        }

        return nearest;
    }

    private bool IsAdjacentToTarget()
    {
        Vector2Int delta = target.CurrentTile - npc.CurrentTile;
        return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) <= 1;
    }

    private Vector2Int SelectApproachTile()
    {
        MapData mapData = World.Instance.MapData;
        List<Vector2Int> candidates = new List<Vector2Int>();

        foreach (Vector2Int offset in AdjacentOffsets)
        {
            Vector2Int tile = target.CurrentTile + offset;

            if (tile.x < 0 || tile.x >= mapData.resolution.x) continue;
            if (tile.y < 0 || tile.y >= mapData.resolution.y) continue;
            if (!Movement.MovableTypes.Contains(mapData.fieldTypes[tile.y, tile.x])) continue;
            if (!World.Instance.MapRuntime.IsEmpty(tile)) continue;

            candidates.Add(tile);
        }

        if (candidates.Count == 0) return npc.CurrentTile;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}
