using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stay", story: "[Agent] Stay On Current Tile", category: "Action", id: "8c16975eab4422b81b0936a88f84da0e")]
public partial class StayAction : Action
{
    [SerializeReference] public BlackboardVariable<NPC> Agent;
    private const float MinStayDuration = 1.0f;
    private const float MaxStayDuration = 3.0f;

    private float stayDuration;
    private float elapsed;

    protected override Status OnStart()
    {
        if (Agent?.Value == null) return Status.Failure;

        stayDuration = UnityEngine.Random.Range(MinStayDuration, MaxStayDuration);
        elapsed = 0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        elapsed += Time.deltaTime;
        return elapsed >= stayDuration ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

