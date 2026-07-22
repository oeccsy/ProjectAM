using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 시야 안의 대화 가능한 NPC 중 가장 가까운 대상을 불러 짝을 맺는다.
/// 부르기까지만 담당하며, 접근과 대화는 각각 다른 노드가 이어받는다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Call", story: "[Agent] Call [TargetNPC]", category: "Action", id: "a5406561013947e04ca17131bb88706d")]
public partial class CallAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;

    [SerializeReference]
    public BlackboardVariable<NPC> TargetNPC;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        Conversation conversation = npc.Conversation;
        if (conversation == null) return Status.Failure;
        if (!conversation.IsTalkable) return Status.Failure;

        NPC target = SelectTarget(npc);
        if (target == null) return Status.Failure;

        Conversation.Call(npc, target);
        if (conversation.Partner != target) return Status.Failure;

        TargetNPC.Value = target;

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd() { }

    // 시야 안에서 아직 아무와도 엮이지 않은 NPC 중 가장 가까운 대상
    private NPC SelectTarget(NPC npc)
    {
        Perception perception = npc.Perception;
        if (perception == null) return null;

        List<NPC> candidates = perception.FindVisibleNpcs();

        NPC nearest = null;
        float nearestSqrDist = float.MaxValue;
        Vector3 origin = npc.transform.position;

        foreach (NPC candidate in candidates)
        {
            if (!candidate.Conversation.IsTalkable) continue;

            float sqrDist = (candidate.transform.position - origin).sqrMagnitude;
            if (sqrDist >= nearestSqrDist) continue;

            nearest = candidate;
            nearestSqrDist = sqrDist;
        }

        return nearest;
    }
}
