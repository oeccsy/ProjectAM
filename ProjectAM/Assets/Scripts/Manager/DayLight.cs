using UnityEngine;
using UnityEngine.Rendering;

// TimeSystem을 읽어 시간대에 따라 조명을 바군다.
// 해를 움직이지 않고 색감만 바꿔 밤낮을 구분한다. 태양 각도는 인스펙터에서 고정한 값을 그대로 쓴다.
public class DayLight : MonoBehaviour
{
    [SerializeField]
    private Light sun;
    // 목표 색으로 수렴하는 데 걸리는 시간 상수. 클수록 천천히 변한다.
    [SerializeField]
    private float blendSeconds = 5f;

    [Header("Sun Color")]
    [SerializeField]
    private Color dayColor = new Color(1.00f, 0.96f, 0.85f);
    [SerializeField]
    private Color duskColor = new Color(1.00f, 0.62f, 0.38f);
    [SerializeField]
    private Color eveningColor = new Color(0.62f, 0.58f, 0.85f);
    [SerializeField]
    private Color nightColor = new Color(0.50f, 0.58f, 0.90f);

    [Header("Sun Intensity")]
    [SerializeField]
    private float dayIntensity = 1.10f;
    [SerializeField]
    private float duskIntensity = 0.90f;
    [SerializeField]
    private float eveningIntensity = 0.70f;
    [SerializeField]
    private float nightIntensity = 0.55f;

    [Header("Ambient")]
    [SerializeField]
    private Color dayAmbient = new Color(0.60f, 0.62f, 0.66f);
    [SerializeField]
    private Color duskAmbient = new Color(0.55f, 0.47f, 0.48f);
    [SerializeField]
    private Color eveningAmbient = new Color(0.36f, 0.37f, 0.50f);
    [SerializeField]
    private Color nightAmbient = new Color(0.24f, 0.26f, 0.42f);

    private bool hasApplied;

    public void BindSun(Light value) => sun = value;

    private void Awake()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
    }

    private void Update()
    {
        TimeSystem time = World.Instance.Time;
        if (time == null) return;

        ApplyPhase(time.Phase);
    }

    private void ApplyPhase(TimePhase phase)
    {
        Color targetSunColor = EvaluateSunColor(phase);
        float targetIntensity = EvaluateIntensity(phase);
        Color targetAmbient = EvaluateAmbient(phase);

        // 첫 프레임은 보간 없이 맞춰 시작 색이 서서히 밝아오는 것을 막는다.
        float blend = hasApplied ? 1f - Mathf.Exp(-Time.deltaTime / blendSeconds) : 1f;
        hasApplied = true;

        if (sun != null)
        {
            sun.color = Color.Lerp(sun.color, targetSunColor, blend);
            sun.intensity = Mathf.Lerp(sun.intensity, targetIntensity, blend);
        }

        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbient, blend);
    }

    private Color EvaluateSunColor(TimePhase phase)
    {
        if (phase == TimePhase.Day) return dayColor;
        if (phase == TimePhase.Dusk) return duskColor;
        if (phase == TimePhase.Evening) return eveningColor;

        return nightColor;
    }

    private float EvaluateIntensity(TimePhase phase)
    {
        if (phase == TimePhase.Day) return dayIntensity;
        if (phase == TimePhase.Dusk) return duskIntensity;
        if (phase == TimePhase.Evening) return eveningIntensity;

        return nightIntensity;
    }

    private Color EvaluateAmbient(TimePhase phase)
    {
        if (phase == TimePhase.Day) return dayAmbient;
        if (phase == TimePhase.Dusk) return duskAmbient;
        if (phase == TimePhase.Evening) return eveningAmbient;

        return nightAmbient;
    }
}
