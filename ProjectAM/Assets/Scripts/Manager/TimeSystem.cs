using UnityEngine;
using System.Collections;

/// <summary>
/// 시간 정보 day, hour 흐름을 관리하는 클래스.
/// 외부에 의해 TimePhase가 전환되기 전에는 시간은 일정 구간 이상 흐르지 않는다.
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
    private WaitUntil waitUntilDay;
    private WaitUntil waitUntilDusk;
    private WaitUntil waitUntilEvening;
    private WaitUntil waitUntilNight;
    private Coroutine timeRoutine;

    public int Day => day;
    public int Hour => hour;
    public TimePhase Phase => timePhase;

    private void Awake()
    {
        hourTick = new WaitForSeconds(secondsPerHour);
        waitUntilDay = new WaitUntil(() => timePhase == TimePhase.Day);
        waitUntilDusk = new WaitUntil(() => timePhase == TimePhase.Dusk);
        waitUntilEvening = new WaitUntil(() => timePhase == TimePhase.Evening);
        waitUntilNight = new WaitUntil(() => timePhase == TimePhase.Night);
        timeRoutine = StartCoroutine(TimeRoutine());
    }

    public void AdvancePhase()
    {
        timePhase = (TimePhase)(((int)timePhase + 1) % 4);
    }

    private IEnumerator TimeRoutine()
    {
        while (true)
        {
            yield return FlowHourUntil(8);

            yield return waitUntilDay;
            yield return FlowHourUntil(17);

            yield return waitUntilDusk;
            yield return FlowHourUntil(19);

            yield return waitUntilEvening;
            yield return FlowHourUntil(21);

            yield return waitUntilNight;
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
