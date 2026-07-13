using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC가 알고 있는 정보들의 저장소. 같은 정보는 한 번만 기억한다.
/// </summary>
public class NpcMemory : MonoBehaviour
{
    private readonly List<ContactInfo> contactInfoList = new List<ContactInfo>();
    private readonly List<VictimInfo> victimInfoList = new List<VictimInfo>();

    public IReadOnlyList<ContactInfo> ContactInfoList => contactInfoList;
    public IReadOnlyList<VictimInfo> VictimInfoList => victimInfoList;

    public void Remember(ContactInfo clue)
    {
        if (contactInfoList.Contains(clue)) return;

        contactInfoList.Add(clue);
    }

    public void Remember(VictimInfo clue)
    {
        if (victimInfoList.Contains(clue)) return;

        victimInfoList.Add(clue);
    }
}