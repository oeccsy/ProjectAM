using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[Condition(name: "IsInHouse", story: "[Agent] Is In House", category: "Condition", id: "d9f5a1c3e7b2486f9a0c1d2e3f4a5b6c")]
public partial class IsInHouseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    public override bool IsTrue()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return false;
        if (npc.HouseEntry == null) return false;

        return npc.HouseEntry.CurrentHouse != null;
    }
}
