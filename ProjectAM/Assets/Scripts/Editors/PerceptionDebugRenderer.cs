#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// NPC에 Perception과 함께 붙여두면, 씬에서 시야 부채꼴 범위를 시각화한다.
/// </summary>
[RequireComponent(typeof(Perception))]
public class PerceptionDebugRenderer : MonoBehaviour
{
    [SerializeField]
    private Color fanColor = new Color(1f, 0.9f, 0.2f, 0.12f);
    [SerializeField]
    private Color edgeColor = new Color(1f, 0.9f, 0.2f, 0.8f);
    [SerializeField]
    private Color visibleColor = new Color(1f, 0.3f, 0.3f, 0.9f);
    [SerializeField]
    private float heightOffset = 0.05f;

    private Perception perception;

    private Perception Perception
    {
        get
        {
            if (perception == null) perception = GetComponent<Perception>();
            return perception;
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawFan();
    }

    private void DrawFan()
    {
        if (Perception == null) return;

        float radius = Perception.ViewRadius;
        float angle = Perception.ViewAngle;

        Vector3 center = transform.position + Vector3.up * heightOffset;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 leftDir = Quaternion.Euler(0f, -angle * 0.5f, 0f) * forward;
        Vector3 rightDir = Quaternion.Euler(0f, angle * 0.5f, 0f) * forward;

        // 채워진 부채꼴
        Handles.color = fanColor;
        Handles.DrawSolidArc(center, Vector3.up, leftDir, angle, radius);

        // 부채꼴 외곽선
        Gizmos.color = edgeColor;
        Gizmos.DrawLine(center, center + leftDir * radius);
        Gizmos.DrawLine(center, center + rightDir * radius);

        const int segments = 24;
        Vector3 prev = center + leftDir * radius;
        for (int i = 1; i <= segments; i++)
        {
            float t = angle * (i / (float)segments);
            Vector3 dir = Quaternion.Euler(0f, -angle * 0.5f + t, 0f) * forward;
            Vector3 cur = center + dir * radius;
            Gizmos.DrawLine(prev, cur);
            prev = cur;
        }

        // 플레이 중이면 실제로 인지 중인 NPC까지 연결선 렌더
        if (!Application.isPlaying) return;

        Gizmos.color = visibleColor;
        foreach (NPC npc in Perception.FindVisibleNpcs())
        {
            if (npc == null) continue;
            Vector3 targetPos = npc.transform.position + Vector3.up * heightOffset;
            Gizmos.DrawLine(center, targetPos);
            Gizmos.DrawWireSphere(targetPos, 0.15f);
        }
    }
}

#endif