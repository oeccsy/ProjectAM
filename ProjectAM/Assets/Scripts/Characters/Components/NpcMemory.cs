using System.Collections.Generic;
using UnityEngine;

// NPC가 알고 있는 단서들의 저장소. 같은 단서는 한 번만 기억한다.
public class NpcMemory : MonoBehaviour
{
    private readonly List<ContactClue> contactClues = new List<ContactClue>();
    private readonly List<VictimClue> victimClues = new List<VictimClue>();

    public IReadOnlyList<ContactClue> ContactClues => contactClues;
    public IReadOnlyList<VictimClue> VictimClues => victimClues;

    public void Remember(ContactClue clue)
    {
        if (contactClues.Contains(clue)) return;

        contactClues.Add(clue);
    }

    public void Remember(VictimClue clue)
    {
        if (victimClues.Contains(clue)) return;

        victimClues.Add(clue);
    }

    // 소문 교환: 아는 단서 중 하나를 무작위로 골라 상대에게 전한다.
    public void ShareRandomClueWith(NpcMemory other)
    {
        int totalCount = contactClues.Count + victimClues.Count;
        if (totalCount == 0) return;

        int index = Random.Range(0, totalCount);

        if (index < contactClues.Count)
        {
            other.Remember(contactClues[index]);
        }
        else
        {
            other.Remember(victimClues[index - contactClues.Count]);
        }
    }
}
