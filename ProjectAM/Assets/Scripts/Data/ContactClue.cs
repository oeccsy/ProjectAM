// 단서: 두 NPC가 접촉했다 (직접 접촉 / 목격 / 전해 들음)
public readonly struct ContactClue
{
    public readonly int day;
    public readonly int hour;
    public readonly NpcColor npcA;
    public readonly NpcColor npcB;

    public ContactClue(int day, int hour, NpcColor npcA, NpcColor npcB)
    {
        // 쌍 순서를 정규화해 같은 접촉이 두 번 기억되지 않게 한다
        if (npcA > npcB) (npcA, npcB) = (npcB, npcA);

        this.day = day;
        this.hour = hour;
        this.npcA = npcA;
        this.npcB = npcB;
    }
}
