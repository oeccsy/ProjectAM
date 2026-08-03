using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// NPC가 다른 NPC에게 불려 대화 상대로 묶여 있는지 확인
/// </summary>
[Serializable, GeneratePropertyBag]
[Condition(name: "IsCalled", story: "[Agent] Is Called", category: "Condition", id: "e4b7c2d1a5f36908b3c4d5e6f7a8b9c0")]
public partial class IsCalledCondition : Condition
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    public override bool IsTrue()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return false;

        Conversation conversation = npc.Conversation;
        if (conversation == null) return false;

        return conversation.ConversationRole == ConversationRole.Callee;
    }
}
