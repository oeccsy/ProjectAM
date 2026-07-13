using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// 현재 시간 정보를 블랙보드 변수에 담는 액션.
/// 시간대 분기(Switch) 직전에 두면 그래프가 평가 시점의 최신 시간을 갖게 된다.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckTime", story: "Check current time into [TimePhase] [Day] [Hour]", category: "Action", id: "a7f3c9e1b5d2648a0c1e2f3a4b5c6d7e")]
public partial class CheckTimeAction : Action
{
    [SerializeReference]
    public BlackboardVariable<int> Day;
    [SerializeReference]
    public BlackboardVariable<int> Hour;
    [SerializeReference]
    public BlackboardVariable<TimePhase> TimePhase;

    protected override Status OnStart()
    {
        TimeSystem time = World.Instance.Time;
        if (time == null) return Status.Failure;

        if (Day != null) Day.Value = time.Day;
        if (Hour != null) Hour.Value = time.Hour;
        if (TimePhase != null) TimePhase.Value = time.Phase;

        return Status.Success;
    }
}
