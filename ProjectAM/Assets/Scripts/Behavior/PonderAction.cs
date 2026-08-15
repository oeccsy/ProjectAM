using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 잠시 멈춰 기억을 되짚어본다. 남은 용의자가 한 명뿐이면 확신하고 그 대상을 남긴다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Ponder", story: "[Agent] Ponder About [TargetNPC]", category: "Action", id: "c6e1f8a3d5b7492c0e4f6a8b1d3c5e7f")]
public partial class PonderAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;
    [SerializeReference]
    public BlackboardVariable<NPC> TargetNPC;

    private const float PonderMin = 1.0f;
    private const float PonderMax = 3.0f;

    private NPC npc;
    private float duration;
    private float elapsed;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;
        if (npc.Suspicion == null) return Status.Failure;

        duration = UnityEngine.Random.Range(PonderMin, PonderMax);
        elapsed = 0.0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        elapsed += Time.deltaTime;
        if (elapsed < duration) return Status.Running;

        NpcColor suspect = npc.Suspicion.FindMostSuspicious();
        if (suspect == NpcColor.Count) return Status.Failure;
        if (!npc.Suspicion.IsConvinced(suspect)) return Status.Failure;

        NPC target = World.Instance.NPCs.Get(suspect);
        if (target == null) return Status.Failure;
        if (!target.Life.IsAlive) return Status.Failure;

        if (TargetNPC != null) TargetNPC.Value = target;
        Debug.Log($"[확신] {npc.OwnColor} : {suspect}가 범인이라고 결론");

        return Status.Success;
    }

    protected override void OnEnd() { }
}
