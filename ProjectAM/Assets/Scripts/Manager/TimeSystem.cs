using UnityEngine;

// 24시간제 게임 내 시계. SimulationScene이 생성해 World.Time에 할당한다.
public class TimeSystem : MonoBehaviour
{
    [SerializeField]
    private float dayLengthSeconds = 120f;
    [SerializeField]
    private float startHour = 8f;
    [SerializeField]
    private float currentHour;
    
    public float Hour => currentHour;
    public TimePhase Phase
    {
        get
        {
            if (currentHour < 5f) return TimePhase.Night;
            if (currentHour < 7f) return TimePhase.Dawn;
            if (currentHour < 9f) return TimePhase.Morning;
            if (currentHour < 16f) return TimePhase.Day;
            if (currentHour < 18f) return TimePhase.Dusk;
            if (currentHour < 20f) return TimePhase.Evening;
            return TimePhase.Night;
        }
    }

    private void Awake()
    {
        currentHour = Mathf.Repeat(startHour, 24f);
    }

    private void Update()
    {
        if (dayLengthSeconds <= 0f) return;
        currentHour = Mathf.Repeat(currentHour + Time.deltaTime * (24f / dayLengthSeconds), 24f);
    }
}
