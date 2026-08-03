using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// Call로 맺어진 역할에 따라 Caller/Callee 대화 루틴을 실행하고, 끝날 때까지 기다린다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Talk", story: "[Agent] Talk With Another", category: "Action", id: "d11dca2152f55263d95bb25ebb4e3199")]
public partial class TalkAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    private NPC npc;
    private Conversation conversation;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        conversation = npc.Conversation;
        if (conversation == null) return Status.Failure;
        if (conversation.Partner == null) return Status.Failure;

        if (conversation.ConversationRole == ConversationRole.Caller)
        {
            conversation.StartCallerRoutine();
        }
        else if (conversation.ConversationRole == ConversationRole.Callee)
        {
            conversation.StartCalleeRoutine();
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (conversation.ConversationRole == ConversationRole.Caller) return Status.Running;
        if (conversation.ConversationRole == ConversationRole.Callee) return Status.Running;

        return Status.Success;
    }

    protected override void OnEnd() { }
}
