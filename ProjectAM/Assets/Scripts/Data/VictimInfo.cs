/// <summary>
/// 누군가 희생되었다는 정보
/// </summary>
public readonly struct VictimInfo
{
    public readonly NpcColor victim;
    public readonly int foundDay;

    public VictimInfo(NpcColor victim, int foundDay)
    {
        this.victim = victim;
        this.foundDay = foundDay;
    }
}