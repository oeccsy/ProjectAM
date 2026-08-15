using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자기 기억만으로 범인을 추론하는 클래스.
/// 희생자와 접촉했던 것으로 기억하는 사람에게 의심을 쌓는다.
/// </summary>
public class Suspicion : MonoBehaviour
{
    private NPC owner;
    private NpcMemory ownMemory;

    private void Awake()
    {
        owner = GetComponent<NPC>();
        ownMemory = GetComponent<NpcMemory>();
    }

    public int CalculateSuspicion(NpcColor color)
    {
        if (color == owner.OwnColor) return 0;

        List<NpcColor> victims = CollectKnownVictims();
        if (victims.Contains(color)) return 0;

        int score = 0;

        foreach (NpcColor victim in victims)
        {
            foreach (ContactInfo contact in ownMemory.ContactInfoList)
            {
                if (!contact.Involves(victim)) continue;
                if (contact.OtherOf(victim) != color) continue;

                score++;
            }
        }

        return score;
    }

    // 의심이 가장 큰 사람. 아무도 의심스럽지 않으면 NpcColor.Count
    public NpcColor FindMostSuspicious()
    {
        List<NpcColor> tops = new List<NpcColor>();
        int topScore = 0;

        foreach (NPC npc in World.Instance.NPCs.All)
        {
            int score = CalculateSuspicion(npc.OwnColor);

            if (score <= 0) continue;
            if (score < topScore) continue;

            if (score > topScore)
            {
                topScore = score;
                tops.Clear();
            }

            tops.Add(npc.OwnColor);
        }

        if (tops.Count == 0) return NpcColor.Count;

        return tops[Random.Range(0, tops.Count)];
    }

    // 같은 죽음이 발견일별로 여러 번 기억되므로 희생자 기준으로 한 번만 센다
    private List<NpcColor> CollectKnownVictims()
    {
        List<NpcColor> victims = new List<NpcColor>();

        foreach (VictimInfo victim in ownMemory.VictimInfoList)
        {
            if (victims.Contains(victim.victim)) continue;

            victims.Add(victim.victim);
        }

        return victims;
    }
}
