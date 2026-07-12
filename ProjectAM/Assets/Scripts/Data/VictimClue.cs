// 단서: 누군가 희생되었다
public readonly struct VictimClue
{
    public readonly NpcColor victim;
    public readonly int foundDay;

    public VictimClue(NpcColor victim, int foundDay)
    {
        this.victim = victim;
        this.foundDay = foundDay;
    }
}
