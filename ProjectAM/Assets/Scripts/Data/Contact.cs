// NPC 두 명이 접촉(대화)했다는 사실 하나.
public readonly struct Contact
{
    public readonly NpcColor first;
    public readonly NpcColor second;

    public Contact(NpcColor first, NpcColor second)
    {
        this.first = first;
        this.second = second;
    }

    public bool Involves(NpcColor color) => first == color || second == color;

    public NpcColor OtherOf(NpcColor color) => (first == color) ? second : first;
}
