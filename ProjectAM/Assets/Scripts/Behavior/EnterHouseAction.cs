using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnterHouse", story: "[Agent] Enter House", category: "Action", id: "c8e4f0a2b3d5597e9f1a2b3c4d5e6f70")]
public partial class EnterHouseAction : Action
{
    [SerializeReference]
    public BlackboardVariable<NPC> Agent;
    [SerializeReference]
    public BlackboardVariable<House> TargetHouse;

    private NPC npc;
    private House house;
    private bool entered;

    protected override Status OnStart()
    {
        npc = Agent?.Value;
        if (npc == null) return Status.Failure;

        house = TargetHouse?.Value;
        if (house == null) house = World.Instance.Houses.Get(npc.OwnColor);
        if (house == null) return Status.Failure;

        NpcAnimation animation = npc.NpcAnimation;
        if (animation == null) return Status.Failure;
        if (npc.HouseEntry == null) return Status.Failure;

        entered = false;
        animation.SpinWithHideAnim(() => entered = true);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!entered) return Status.Running;

        npc.HouseEntry.Enter(house);
        return Status.Success;
    }

    protected override void OnEnd() { }
}
