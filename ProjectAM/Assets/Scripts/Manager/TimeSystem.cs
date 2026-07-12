using System;
using UnityEngine;

// 24시간제 게임 내 시계. SimulationScene이 생성해 World.Time에 할당한다.
public class TimeSystem : MonoBehaviour
{
    [SerializeField]
    private float dayLengthSeconds = 300f;
    [SerializeField]
    private float startHour = 8f;
    [SerializeField]
    private float currentHour;
    [SerializeField]
    private int day = 1;

    public event Action<TimePhase> PhaseChanged;

    public float Hour => currentHour;
    public int Day => day;
    public TimePhase Phase => EvaluatePhase(currentHour);

    private void Awake()
    {
        currentHour = Mathf.Repeat(startHour, 24f);
    }

    private void Update()
    {
        if (dayLengthSeconds <= 0f) return;

        TimePhase previousPhase = Phase;
        float previousHour = currentHour;

        currentHour = Mathf.Repeat(currentHour + Time.deltaTime * (24f / dayLengthSeconds), 24f);

        if (currentHour < previousHour) day++;
        if (Phase != previousPhase) PhaseChanged?.Invoke(Phase);
    }

    private TimePhase EvaluatePhase(float hour)
    {
        if (hour < 7f) return TimePhase.Night;
        if (hour < 16f) return TimePhase.Day;
        if (hour < 18f) return TimePhase.Dusk;
        if (hour < 20f) return TimePhase.Evening;
        return TimePhase.Night;
    }
}
