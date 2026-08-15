using UnityEngine;
using System.Collections;

/// <summary>
/// 시간 정보 day, hour, timePhase를 보관하고 알려주는 클래스.
/// 언제 다음 단계로 넘길지는 SimulationFlow가 정하고, 이 클래스는 그 지시를 받아 값을 옮긴다.
/// </summary>
public class TimeSystem : MonoBehaviour
{
    [SerializeField]
    private int day = 1;
    [SerializeField]
    private int hour = 8;
    [SerializeField]
    private TimePhase timePhase = TimePhase.Day;
    [SerializeField]
    private float secondsPerHour = 10f;

    private WaitForSeconds hourTick;

    public int Day => day;
    public int Hour => hour;
    public TimePhase Phase => timePhase;

    private void Awake()
    {
        hourTick = new WaitForSeconds(secondsPerHour);
    }

    // 한 시간이 몇 초에 흐를지 바꾼다. 얼마로 둘지는 SimulationFlow가 정한다
    public void ApplySecondsPerHour(float seconds)
    {
        secondsPerHour = seconds;
        hourTick = new WaitForSeconds(seconds);
    }

    public void ApplyPhase(TimePhase phase)
    {
        timePhase = phase;
    }

    public void NextDay()
    {
        day++;
        hour = 0;
    }

    // 목표 시각까지 시간을 흘려보낸다. 이미 지났으면 즉시 끝난다
    public IEnumerator FlowHourUntil(int targetHour)
    {
        while (hour < targetHour)
        {
            yield return hourTick;
            hour++;
        }
    }
}
