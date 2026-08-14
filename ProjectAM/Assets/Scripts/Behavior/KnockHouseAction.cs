using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 문 앞에서 집 주인이 무사한지 확인한다. 확인 결과는 Witness가 메모리에 남긴다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "KnockHouse", story: "[Agent] Knock [TargetHouse]", category: "Action", id: "f3c8d5e0a2b4586f9c1d3e5f7a9b2c4d")]
public partial class KnockHouseAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;
    [SerializeReference]
    public BlackboardVariable<House> TargetHouse;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        House house = TargetHouse?.Value;
        if (house == null) return Status.Failure;
        if (npc.Witness == null) return Status.Failure;

        npc.Witness.ObserveHouse(house);

        return Status.Success;
    }
}
