using System.Collections.Generic;
using UnityEngine;

// 밤 사건: 오늘 낮 범인과 접촉했던 이들 중 한 명을 무작위로 고른다.
public class NightEvent
{
    public NPC SelectVictim()
    {
        NPC culprit = FindAliveCulprit();
        if (culprit == null) return null;

        List<NPC> candidates = new List<NPC>();

        foreach (NpcColor color in World.Instance.Contacts.FindContactsOf(culprit.OwnColor))
        {
            NPC partner = World.Instance.NPCs.Get(color);
            if (partner == null) continue;
            if (!partner.IsAlive) continue;

            candidates.Add(partner);
        }

        if (candidates.Count == 0) return null;   // 접촉이 없었다면 조용한 밤

        return candidates[Random.Range(0, candidates.Count)];
    }

    private NPC FindAliveCulprit()
    {
        foreach (NPC npc in World.Instance.NPCs.All)
        {
            if (npc.Role != Role.Witch) continue;
            if (!npc.IsAlive) continue;

            return npc;
        }

        return null;
    }
}
