using System.Collections.Generic;

public class Registry<T, TKey> where T : class
{
    private readonly List<T> elems = new();
    private readonly Dictionary<TKey, T> byKey = new();

    public IReadOnlyList<T> All => elems;
    public T Get(TKey key) => byKey.TryGetValue(key, out T item) ? item : null;

    public void Register(TKey key, T elem)
    {
        elems.Add(elem);
        byKey[key] = elem;
    }
}