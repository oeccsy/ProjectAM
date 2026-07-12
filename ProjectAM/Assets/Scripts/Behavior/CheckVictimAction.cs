using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// 찾아간 집의 주인이 희생된 상태라면 그 사실을 단서로 기억한다.
// Sequence [ SelectVisitHouse → ApproachHouse → CheckVictim ] 조합의 마지막 단계.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckVictim", story: "[Agent] Check Victim At [TargetHouse]", category: "Action", id: "d6f2a8b0c4e5861f3a4b5c6d7e8f9a02")]
public partial class CheckVictimAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;
    [SerializeReference] public BlackboardVariable<House> TargetHouse;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        House house = TargetHouse?.Value;
        if (house == null) return Status.Failure;

        NPC owner = World.Instance.NPCs.Get(house.owner);
        if (owner != null && owner.LifeState == LifeState.Victim)
        {
            int day = World.Instance.Time.Day;
            npc.Memory.Remember(new VictimClue(owner.OwnColor, day));
        }

        return Status.Success;
    }
}
