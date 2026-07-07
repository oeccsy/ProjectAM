using System.Collections.Generic;

public class Registry<T> where T : class
{
    private readonly List<T> elems = new();
    private readonly Dictionary<NpcColor, T> byColor = new();

    public IReadOnlyList<T> All => elems;

    public T Get(NpcColor color) => byColor.TryGetValue(color, out T item) ? item : null;

    public void Register(NpcColor color, T elem)
    {
        elems.Add(elem);
        byColor[color] = elem;
    }
}