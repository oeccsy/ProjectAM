using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바라보는 방향 기준 부채꼴 범위 안의 다른 NPC를 인지하는 클래스
/// </summary>
public class Perception : MonoBehaviour
{
    [SerializeField]
    private float viewRadius = 5f;
    [SerializeField]
    private float viewAngle = 120f;

    private NPC owner;
    
    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;

    private void Awake()
    {
        owner = GetComponent<NPC>();
    }

    public bool CanSee(NPC target)
    {
        if (target == null) return false;
        if (target == owner) return false;
        if (!target.Life.IsAlive) return false;
        if (target.HouseEntry.CurrentHouse != null) return false;

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > viewRadius * viewRadius) return false;

        return Vector3.Angle(transform.forward, toTarget) <= viewAngle * 0.5f;
    }

    public List<NPC> FindVisibleNpcs()
    {
        List<NPC> visible = new List<NPC>();

        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (CanSee(npc)) visible.Add(npc);
        }

        return visible;
    }
}