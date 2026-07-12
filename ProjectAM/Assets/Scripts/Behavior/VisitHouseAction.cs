using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// 무작위 타인의 집 마당까지 찾아간다. 집주인이 희생됐다면 그 사실을 알게 된다.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "VisitHouse", story: "[Agent] Visit Another Npc House", category: "Action", id: "a3c9d5e7f1b2639c0d1e2f3a4b5c6d7f")]
public partial class VisitHouseAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    private const float MaxWaitTime = 3.0f;

    private NPC npc;
    private Movement movement;
    private House targetHouse;
    private Vector2Int destTile;
    private float waitTime;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        movement = npc.Movement;
        if (movement.State != MoveState.Idle) return Status.Failure;

        targetHouse = SelectRandomOtherHouse();
        if (targetHouse == null) return Status.Failure;

        destTile = targetHouse.FindRandomYardTile(World.Instance.MapData);
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
            return Status.Running;
        }

        if (npc.CurrentTile != destTile) return Status.Failure;

        CheckOwnerVictim();
        return Status.Success;
    }

    protected override void OnEnd() { }

    // 집에 도착했다. 집주인이 희생된 상태라면 그 사실을 기억한다.
    private void CheckOwnerVictim()
    {
        NPC owner = World.Instance.NPCs.Get(targetHouse.owner);
        if (owner == null) return;
        if (owner.LifeState != LifeState.Victim) return;

        int day = World.Instance.Time.Day;
        npc.Memory.Remember(new VictimClue(owner.OwnColor, day));
    }

    private House SelectRandomOtherHouse()
    {
        List<House> candidates = new List<House>();

        foreach (House house in World.Instance.Houses.All)
        {
            if (house.owner == npc.OwnColor) continue;

            candidates.Add(house);
        }

        if (candidates.Count == 0) return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}
