using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 지면에 빛이 비친 것처럼 보이는 데칼을 생성하고 그 세기를 제어하는 클래스.
/// 광원이 아니라 화면 깊이를 이용해 지면에 투영하는 방식이므로, 벽면이나 공중의 물체는 밝히지 않는다.
/// </summary>
public class LightDecal : MonoBehaviour
{
    private const string MaterialPath = "Materials/LightDecal";
    private const float ZFightMargin = 0.02f;
    private const float GroundSearchHeight = 10f;

    private static readonly int GlowStartAmountId = Shader.PropertyToID("_GlowStartAmount");
    private static readonly int GlowTargetAmountId = Shader.PropertyToID("_GlowTargetAmount");
    private static readonly int GlowStartTimeId = Shader.PropertyToID("_GlowStartTime");
    private static readonly int GlowDurationId = Shader.PropertyToID("_GlowDuration");

    private Renderer decalRenderer;
    private MaterialPropertyBlock decalProperties;

    private float glowStartAmount;
    private float glowTargetAmount;
    private float glowStartTime;

    private void Awake()
    {
        Material decalMaterial = Resources.Load<Material>(MaterialPath);
        if (decalMaterial == null)
        {
            Debug.LogWarning($"빛 데칼 머티리얼을 찾지 못함 : {MaterialPath}");
            return;
        }

        decalRenderer = GetComponent<Renderer>();
        decalRenderer.sharedMaterial = decalMaterial;
        decalRenderer.shadowCastingMode = ShadowCastingMode.Off;
        decalRenderer.receiveShadows = false;

        decalProperties = new MaterialPropertyBlock();
        StopGlow();

        Vector3 groundPosition = transform.position;
        groundPosition.y = FindGroundHeight(groundPosition) + ZFightMargin;
        transform.position = groundPosition;

        Destroy(GetComponent<Collider>());
    }

    public static LightDecal Create(Transform parent, Vector3 position, float size)
    {
        GameObject decalObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        decalObject.name = nameof(LightDecal);

        decalObject.transform.SetParent(parent, false);
        decalObject.transform.position = position;
        decalObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        decalObject.transform.localScale = new Vector3(size, size, 1f);

        return decalObject.AddComponent<LightDecal>();
    }

    private float FindGroundHeight(Vector3 position)
    {
        RaycastHit[] hits = Physics.RaycastAll(position + Vector3.up * GroundSearchHeight, Vector3.down, GroundSearchHeight * 2f);

        float groundHeight = float.NegativeInfinity;
        foreach (RaycastHit hit in hits)
        {
            // 데칼이 달린 구조물 충돌 제외
            if (hit.collider.transform.IsChildOf(transform.parent)) continue;
            if (hit.point.y > groundHeight) groundHeight = hit.point.y;
        }

        if (float.IsNegativeInfinity(groundHeight)) return position.y;

        return groundHeight;
    }

    public void StartGlow()
    {
        // 연속적인 변화를 위한 현재 밝기 확인
        float actualDuration = decalRenderer.sharedMaterial.GetFloat(GlowDurationId) * Mathf.Abs(glowTargetAmount - glowStartAmount);
        float elapsed = Time.timeSinceLevelLoad - glowStartTime;
        float progress = actualDuration > 0f ? Mathf.Clamp01(elapsed / actualDuration) : 1f;
        float glowAmount = Mathf.Lerp(glowStartAmount, glowTargetAmount, progress);

        glowStartAmount = glowAmount;
        glowTargetAmount = 1f;
        glowStartTime = Time.timeSinceLevelLoad;

        decalProperties.SetFloat(GlowStartAmountId, glowStartAmount);
        decalProperties.SetFloat(GlowTargetAmountId, glowTargetAmount);
        decalProperties.SetFloat(GlowStartTimeId, glowStartTime);
        decalRenderer.SetPropertyBlock(decalProperties);
    }

    public void StopGlow()
    {
        // 연속적인 변화를 위한 현재 밝기 확인
        float actualDuration = decalRenderer.sharedMaterial.GetFloat(GlowDurationId) * Mathf.Abs(glowTargetAmount - glowStartAmount);
        float elapsed = Time.timeSinceLevelLoad - glowStartTime;
        float progress = actualDuration > 0f ? Mathf.Clamp01(elapsed / actualDuration) : 1f;
        float glowAmount = Mathf.Lerp(glowStartAmount, glowTargetAmount, progress);

        glowStartAmount = glowAmount;
        glowTargetAmount = 0f;
        glowStartTime = Time.timeSinceLevelLoad;

        decalProperties.SetFloat(GlowStartAmountId, glowStartAmount);
        decalProperties.SetFloat(GlowTargetAmountId, glowTargetAmount);
        decalProperties.SetFloat(GlowStartTimeId, glowStartTime);
        decalRenderer.SetPropertyBlock(decalProperties);
    }
}
