/// <summary>
/// 두 NPC가 접촉했다는 정보
/// </summary>
public readonly struct ContactClue
{
    public readonly int day;
    public readonly int hour;
    public readonly NpcColor npcA;
    public readonly NpcColor npcB;

    public ContactClue(int day, int hour, NpcColor npcA, NpcColor npcB)
    {
        // 쌍 순서를 정규화해 같은 정보가 두 번 기억되지 않게 함
        if (npcA > npcB) (npcA, npcB) = (npcB, npcA);

        this.day = day;
        this.hour = hour;
        this.npcA = npcA;
        this.npcB = npcB;
    }

    public bool Involves(NpcColor color) => npcA == color || npcB == color;

    public NpcColor OtherOf(NpcColor color) => (npcA == color) ? npcB : npcA;
}