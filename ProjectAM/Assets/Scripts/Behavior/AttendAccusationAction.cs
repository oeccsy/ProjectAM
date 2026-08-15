using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 고발이 열려 있으면 청중으로 참여한다. 절차가 끝날 때까지 이 노드가 붙잡고 있는다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttendAccusation", story: "[Agent] Attend Accusation", category: "Action", id: "d7f2a9b4e6c8503d1f5a7b9c2e4d6f8a")]
public partial class AttendAccusationAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;
        if (npc.Accusation == null) return Status.Failure;

        if (Accusation.Phase == AccusationPhase.None) return Status.Failure;
        if (Accusation.Accuser == npc) return Status.Failure;

        npc.Accusation.StartAudienceRoutine();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return (Accusation.Phase == AccusationPhase.None) ? Status.Success : Status.Running;
    }

    protected override void OnEnd() { }
}
