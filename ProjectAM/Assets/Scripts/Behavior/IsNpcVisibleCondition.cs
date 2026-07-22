using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// NPC가 시야(Perception) 안에서 다른 NPC를 발견했는지 확인
/// </summary>
[Serializable, GeneratePropertyBag]
[Condition(name: "Is Npc Visible", story: "[Agent] Sees Another Npc", category: "Condition", id: "c3e8f1a4b6d05927e1f3a4b5c6d7e8f0")]
public partial class IsNpcVisibleCondition : Condition
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    public override bool IsTrue()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return false;

        Perception perception = npc.Perception;
        if (perception == null) return false;

        return perception.FindVisibleNpcs().Count > 0;
    }
}
