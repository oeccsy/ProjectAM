using UnityEngine;
using UnityEngine.Rendering;

// TimeSystem을 읽어 시간대에 따라 조명을 바군다.
public class DayLight : MonoBehaviour
{
    [SerializeField]
    private Light sun;
    [SerializeField]
    private float maxIntensity = 1.2f;

    [Header("Sun Color")]
    [SerializeField]
    private Color nightColor = new Color(0.10f, 0.15f, 0.35f);
    [SerializeField]
    private Color dawnColor  = new Color(1.00f, 0.55f, 0.40f);
    [SerializeField]
    private Color dayColor   = new Color(1.00f, 0.96f, 0.85f);
    [SerializeField]
    private Color duskColor  = new Color(1.00f, 0.45f, 0.30f);

    [Header("Ambient")]
    [SerializeField]
    private Color dayAmbient   = new Color(0.55f, 0.55f, 0.60f);
    [SerializeField]
    private Color nightAmbient = new Color(0.05f, 0.07f, 0.15f);

    public void BindSun(Light value) => sun = value;
    
    private void Awake()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
    }

    private void Update()
    {
        TimeSystem time = World.Instance.Time;
        if (time == null) return;

        ApplyHour(time.Hour);
    }

    private void ApplyHour(float hour)
    {
        float height = Mathf.Sin((hour - 6f) / 24f * 2f * Mathf.PI);
        float day = Mathf.Clamp01(height);

        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler((hour - 6f) * 15f, 170f, 0f);
            sun.color = EvaluateColor(hour);
            sun.intensity = day * maxIntensity;
        }

        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, day);
    }

    private Color EvaluateColor(float hour)
    {
        if (hour < 5f)  return nightColor;                                            // 밤
        if (hour < 7f)  return Color.Lerp(nightColor, dawnColor, (hour - 5f) / 2f);   // 새벽
        if (hour < 9f)  return Color.Lerp(dawnColor, dayColor, (hour - 7f) / 2f);     // 아침
        if (hour < 16f) return dayColor;                                              // 낮
        if (hour < 18f) return Color.Lerp(dayColor, duskColor, (hour - 16f) / 2f);    // 해질녘
        if (hour < 20f) return Color.Lerp(duskColor, nightColor, (hour - 18f) / 2f);  // 저녁
        return nightColor;                                                            // 밤
    }
}
