using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 확신한 대상을 고발한다. 고발 절차가 끝날 때까지 이 노드가 붙잡고 있는다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Accuse", story: "[Agent] Accuse [TargetNPC]", category: "Action", id: "b5d0e7f2c4a6381b9d3e5f7a0c2b4d6e")]
public partial class AccuseAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;
    [SerializeReference]
    public BlackboardVariable<NPC> TargetNPC;

    private bool opened;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;
        if (npc.Accusation == null) return Status.Failure;

        NPC target = TargetNPC?.Value;
        if (target == null) return Status.Failure;

        // 마을에 고발은 한 번에 하나만 열린다
        if (Accusation.Phase != AccusationPhase.None) return Status.Failure;

        npc.Accusation.StartAccuserRoutine(target);
        opened = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // 루틴이 첫 단계에 들어설 때까지는 아직 None이므로 종료로 오해하지 않는다
        if (!opened)
        {
            if (Accusation.Phase == AccusationPhase.None) return Status.Running;

            opened = true;
        }

        return (Accusation.Phase == AccusationPhase.None) ? Status.Success : Status.Running;
    }

    protected override void OnEnd() { }
}
