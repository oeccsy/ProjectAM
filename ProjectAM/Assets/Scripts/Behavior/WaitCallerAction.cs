using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 부름을 받은 쪽이 하던 이동을 멈추고 상대를 바라보며 기다린다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitCaller", story: "[Agent] Wait Caller", category: "Action", id: "ffbd040cbb3c4ab906d0e80c756913e1")]
public partial class WaitCallerAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;

    private NPC npc;
    private NPC caller;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        Conversation conversation = npc.Conversation;
        if (conversation == null) return Status.Failure;
        if (conversation.ConversationState != ConversationState.Callee) return Status.Failure;

        caller = conversation.Partner;
        if (caller == null) return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 callerDir = caller.transform.position - npc.transform.position;
        callerDir.y = 0.0f;
        npc.Movement.Look = callerDir;

        Vector2Int calleeTile = npc.CurrentTile;
        Vector2Int callerTile = caller.CurrentTile;

        int dx = Mathf.Abs(calleeTile.x - callerTile.x);
        int dy = Mathf.Abs(calleeTile.y - callerTile.y);

        if (dx + dy <= 1) return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd() { }
}
