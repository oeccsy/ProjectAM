using System.Collections.Generic;

// 생존 NPC들이 아는 사실(희생 발견 + 접촉 기록)로부터 개체별 의심치를 집계한다.
// 희생자와 접촉했던 것으로 알려진 개체일수록 의심이 쌓인다.
public class SuspicionEvaluator
{
    public Dictionary<NpcColor, int> Evaluate()
    {
        Dictionary<NpcColor, int> suspicion = new Dictionary<NpcColor, int>();

        foreach (NPC observer in World.Instance.NPCs.All)
        {
            if (!observer.IsAlive) continue;

            AccumulateFrom(observer.Memory, suspicion);
        }

        return suspicion;
    }

    private void AccumulateFrom(NpcMemory memory, Dictionary<NpcColor, int> suspicion)
    {
        HashSet<NpcColor> knownVictims = new HashSet<NpcColor>();

        foreach (MemoryRecord record in memory.Records)
        {
            if (record.type == MemoryType.VictimFound) knownVictims.Add(record.subject);
        }

        if (knownVictims.Count == 0) return;

        foreach (MemoryRecord record in memory.Records)
        {
            if (record.type != MemoryType.ContactSeen) continue;

            AccumulateContact(record.subject, record.partner, knownVictims, suspicion);
            AccumulateContact(record.partner, record.subject, knownVictims, suspicion);
        }
    }

    // victim과 접촉했던 other가 아직 마을에 있다면 의심치를 올린다.
    private void AccumulateContact(NpcColor victim, NpcColor other, HashSet<NpcColor> knownVictims, Dictionary<NpcColor, int> suspicion)
    {
        if (!knownVictims.Contains(victim)) return;
        if (knownVictims.Contains(other)) return;

        NPC suspect = World.Instance.NPCs.Get(other);
        if (suspect == null) return;
        if (!suspect.IsAlive) return;

        suspicion.TryGetValue(other, out int count);
        suspicion[other] = count + 1;
    }
}
