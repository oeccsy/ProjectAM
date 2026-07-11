// NPC 메모리의 사실 하나. 소문 교환의 단위이기도 하다.
public readonly struct MemoryRecord
{
    public readonly MemoryType type;
    public readonly int day;
    public readonly NpcColor subject;   // ContactSeen: 접촉자 한쪽 / VictimFound: 희생자
    public readonly NpcColor partner;   // ContactSeen: 접촉자 다른 쪽 / VictimFound: 미사용

    private MemoryRecord(MemoryType type, int day, NpcColor subject, NpcColor partner)
    {
        this.type = type;
        this.day = day;
        this.subject = subject;
        this.partner = partner;
    }

    // 쌍 순서를 정규화해 같은 접촉이 두 번 기록되지 않게 한다.
    public static MemoryRecord ContactSeen(int day, NpcColor a, NpcColor b)
    {
        if (a > b) (a, b) = (b, a);
        return new MemoryRecord(MemoryType.ContactSeen, day, a, b);
    }

    public static MemoryRecord VictimFound(int day, NpcColor victim)
    {
        return new MemoryRecord(MemoryType.VictimFound, day, victim, victim);
    }
}
