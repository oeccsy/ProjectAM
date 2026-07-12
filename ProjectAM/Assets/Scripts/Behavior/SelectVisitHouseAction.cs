using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// 방문할 무작위 타인의 집을 골라 블랙보드 TargetHouse에 담는다.
// Sequence [ SelectVisitHouse → ApproachHouse → CheckVictim ] 조합의 첫 단계.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SelectVisitHouse", story: "[Agent] Select Another Npc House As [TargetHouse]", category: "Action", id: "c5e1f7a9b3d4750e2f3a4b5c6d7e8f91")]
public partial class SelectVisitHouseAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;
    [SerializeReference] public BlackboardVariable<House> TargetHouse;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;
        if (TargetHouse == null) return Status.Failure;

        List<House> candidates = new List<House>();

        foreach (House house in World.Instance.Houses.All)
        {
            if (house.owner == npc.OwnColor) continue;

            candidates.Add(house);
        }

        if (candidates.Count == 0) return Status.Failure;

        TargetHouse.Value = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        return Status.Success;
    }
}
