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

    private DayLightConfig config;
    private Camera backgroundCamera;
    private bool hasApplied;

    public void BindSun(Light value) => sun = value;

    private void Awake()
    {
        config = Resources.Load<DayLightConfig>("Data/DayLightConfig");
        if (config == null)
        {
            Debug.LogWarning("DayLightConfig not found at Resources/Data/DayLightConfig.");
            return;
        }

        // 윗면과 옆면의 앰비언트가 달라야 형태가 드러난다. Flat이면 모든 면이 같은 값을 받는다
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
    }

    private void Update()
    {
        if (config == null) return;

        TimeSystem time = World.Instance.Time;
        if (time == null) return;

        ApplyPhase(time.Phase);
    }

    private void ApplyPhase(TimePhase phase)
    {
        DayLightConfig.PhaseLighting target = EvaluatePhase(phase);

        // 첫 프레임은 보간 없이 맞춰 시작 색이 서서히 밝아오는 것을 막는다.
        float blend = hasApplied ? 1f - Mathf.Exp(-Time.deltaTime / blendSeconds) : 1f;
        hasApplied = true;

        if (sun != null)
        {
            sun.color = Color.Lerp(sun.color, target.sunColor, blend);
            sun.intensity = Mathf.Lerp(sun.intensity, target.sunIntensity, blend);
        }

        RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, target.ambientSky, blend);
        RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, target.ambientEquator, blend);
        RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, target.ambientGround, blend);

        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, target.fogColor, blend);
        RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, target.fogStart, blend);
        RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, target.fogEnd, blend);

        ApplyBackgroundColor(RenderSettings.fogColor);
    }

    // 배경색이 포그 색과 어긋나면 멀어진 지형이 배경으로 녹아들지 않는다
    private void ApplyBackgroundColor(Color color)
    {
        if (backgroundCamera == null) backgroundCamera = Camera.main;
        if (backgroundCamera == null) return;

        backgroundCamera.backgroundColor = color;
    }

    private DayLightConfig.PhaseLighting EvaluatePhase(TimePhase phase)
    {
        if (phase == TimePhase.Day) return config.day;
        if (phase == TimePhase.Dusk) return config.dusk;
        if (phase == TimePhase.Evening) return config.evening;

        return config.night;
    }
}
