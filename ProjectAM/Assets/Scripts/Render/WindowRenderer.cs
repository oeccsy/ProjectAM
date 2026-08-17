using UnityEngine;

/// <summary>
/// 창문의 발광 세기를 제어하는 클래스.
/// 창문 메시를 찾아 전용 머티리얼로 교체하되, 교체 전의 색을 이어받아 꺼진 상태의 모습을 유지한다.
/// </summary>
public class WindowRenderer : MonoBehaviour
{
    private const string MaterialPath = "Materials/WindowLight";
    private const string WindowObjectName = "Cabin_Window_Big_Glass";

    private static readonly int SourceColorId = Shader.PropertyToID("baseColorFactor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int GlowStartAmountId = Shader.PropertyToID("_GlowStartAmount");
    private static readonly int GlowTargetAmountId = Shader.PropertyToID("_GlowTargetAmount");
    private static readonly int GlowStartTimeId = Shader.PropertyToID("_GlowStartTime");
    private static readonly int GlowDurationId = Shader.PropertyToID("_GlowDuration");

    private Renderer meshRenderer;
    private MaterialPropertyBlock windowProperties;

    private float glowStartAmount;
    private float glowTargetAmount;
    private float glowStartTime;

    private void Awake()
    {
        Material windowMaterial = Resources.Load<Material>(MaterialPath);
        if (windowMaterial == null)
        {
            Debug.LogWarning($"창문 머테리얼을 찾지 못함 : {MaterialPath}");
            return;
        }

        Transform windowTransform = transform.Find(WindowObjectName);
        if (windowTransform == null)
        {
            Debug.LogWarning($"창문 오브젝트를 찾지 못함 : {WindowObjectName}");
            return;
        }

        meshRenderer = windowTransform.GetComponent<Renderer>();

        Color baseColor = meshRenderer.sharedMaterial.GetColor(SourceColorId);
        windowProperties = new MaterialPropertyBlock();
        windowProperties.SetColor(BaseColorId, baseColor);

        meshRenderer.sharedMaterial = windowMaterial;
        StopGlow();
    }

    public void StartGlow()
    {
        // 연속적인 변화를 위한 현재 밝기 확인
        float actualDuration = meshRenderer.sharedMaterial.GetFloat(GlowDurationId) * Mathf.Abs(glowTargetAmount - glowStartAmount);
        float elapsed = Time.timeSinceLevelLoad - glowStartTime;
        float progress = actualDuration > 0f ? Mathf.Clamp01(elapsed / actualDuration) : 1f;
        float glowAmount = Mathf.Lerp(glowStartAmount, glowTargetAmount, progress);

        glowStartAmount = glowAmount;
        glowTargetAmount = 1f;
        glowStartTime = Time.timeSinceLevelLoad;

        windowProperties.SetFloat(GlowStartAmountId, glowStartAmount);
        windowProperties.SetFloat(GlowTargetAmountId, glowTargetAmount);
        windowProperties.SetFloat(GlowStartTimeId, glowStartTime);
        meshRenderer.SetPropertyBlock(windowProperties);
    }

    public void StopGlow()
    {
        // 연속적인 변화를 위한 현재 밝기 확인
        float actualDuration = meshRenderer.sharedMaterial.GetFloat(GlowDurationId) * Mathf.Abs(glowTargetAmount - glowStartAmount);
        float elapsed = Time.timeSinceLevelLoad - glowStartTime;
        float progress = actualDuration > 0f ? Mathf.Clamp01(elapsed / actualDuration) : 1f;
        float glowAmount = Mathf.Lerp(glowStartAmount, glowTargetAmount, progress);

        glowStartAmount = glowAmount;
        glowTargetAmount = 0f;
        glowStartTime = Time.timeSinceLevelLoad;

        windowProperties.SetFloat(GlowStartAmountId, glowStartAmount);
        windowProperties.SetFloat(GlowTargetAmountId, glowTargetAmount);
        windowProperties.SetFloat(GlowStartTimeId, glowStartTime);
        meshRenderer.SetPropertyBlock(windowProperties);
    }
}
