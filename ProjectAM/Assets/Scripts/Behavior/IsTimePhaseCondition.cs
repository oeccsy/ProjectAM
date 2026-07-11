using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

// 행동 트리에서 시간대에 따라 분기하기 위한 조건 (예: 낮에만 상호작용, 저녁이면 귀가)
[Serializable, GeneratePropertyBag]
[Condition(name: "IsTimePhase", story: "current time phase is [Phase]", category: "Conditions", id: "b4d0e6f8a2c3749d1e2f3a4b5c6d7e80")]
public partial class IsTimePhaseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<TimePhase> Phase;

    public override bool IsTrue()
    {
        TimeSystem time = World.Instance.Time;
        if (time == null) return false;

        return time.Phase == Phase.Value;
    }
}
