using System.Collections.Generic;
using UnityEngine;

// NPC가 알고 있는 사실들의 저장소. 중복된 사실은 한 번만 기억한다.
public class NpcMemory : MonoBehaviour
{
    private readonly List<MemoryRecord> records = new List<MemoryRecord>();

    public IReadOnlyList<MemoryRecord> Records => records;

    public void Remember(MemoryRecord record)
    {
        if (records.Contains(record)) return;

        records.Add(record);
    }

    // 소문 교환용: 아는 사실 중 하나를 무작위로 꺼낸다.
    public bool TryPickRandom(out MemoryRecord record)
    {
        if (records.Count == 0)
        {
            record = default;
            return false;
        }

        record = records[Random.Range(0, records.Count)];
        return true;
    }
}
