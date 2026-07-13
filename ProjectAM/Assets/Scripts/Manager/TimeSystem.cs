using UnityEngine;
using System.Collections;

/// <summary>
/// 시간 정보 day, hour, timePhase 흐름을 관리하는 클래스.
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
    private Coroutine timeRoutine;

    public int Day => day;
    public int Hour => hour;
    public TimePhase Phase => timePhase;

    private void Awake()
    {
        hourTick = new WaitForSeconds(secondsPerHour);
        timeRoutine = StartCoroutine(TimeRoutine());
    }

    private IEnumerator TimeRoutine()
    {
        while (true)
        {
            yield return FlowHourUntil(8);
            timePhase = TimePhase.Day;

            yield return FlowHourUntil(17);
            timePhase = TimePhase.Dusk;

            yield return FlowHourUntil(19);
            timePhase = TimePhase.Evening;

            yield return FlowHourUntil(21);
            timePhase = TimePhase.Night;
            
            yield return FlowHourUntil(24);
            day++;
            hour = 0;
        }
    }

    private IEnumerator FlowHourUntil(int targetHour)
    {
        while (hour < targetHour)
        {
            yield return hourTick;
            hour++;
        }
    }
}
