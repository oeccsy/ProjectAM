using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ExitHouse", story: "[Agent] Exit House", category: "Action", id: "d0f6a2b4c8e6608f0a1b2c3d4e5f6a81")]
public partial class ExitHouseAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;

    private bool exited;

    protected override Status OnStart()
    {
        NPC npc = Agent?.Value;
        if (npc == null) return Status.Failure;
        if (npc.HouseEntry == null) return Status.Failure;
        if (npc.HouseEntry.CurrentHouse == null) return Status.Failure;   // 집에 없으면 나갈 것도 없음

        NpcAnimation animation = npc.GetComponent<NpcAnimation>();
        if (animation == null) return Status.Failure;

        npc.HouseEntry.Exit();

        exited = false;
        animation.PlayExitHouse(() => exited = true);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!exited) return Status.Running;

        return Status.Success;
    }

    protected override void OnEnd() { }
}
