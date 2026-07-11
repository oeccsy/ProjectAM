using System.Collections.Generic;

// 오늘 하루 동안 일어난 NPC 간 접촉 기록. 밤 사건 처리 후 비운다.
public class ContactLog
{
    private readonly List<Contact> todayContacts = new List<Contact>();

    public IReadOnlyList<Contact> TodayContacts => todayContacts;

    public void Record(NpcColor first, NpcColor second)
    {
        todayContacts.Add(new Contact(first, second));
    }

    // 오늘 target과 접촉한 상대 목록 (중복 제거)
    public List<NpcColor> FindContactsOf(NpcColor target)
    {
        List<NpcColor> partners = new List<NpcColor>();

        foreach (Contact contact in todayContacts)
        {
            if (!contact.Involves(target)) continue;

            NpcColor other = contact.OtherOf(target);
            if (!partners.Contains(other)) partners.Add(other);
        }

        return partners;
    }

    public void Clear() => todayContacts.Clear();
}
