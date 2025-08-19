using System.Text;

namespace Stellar.Common;

public class Bucket<TKey> where TKey : notnull
{
    private readonly Dictionary<TKey, bool> bucket = [];

    public Bucket(ICollection<TKey> elements)
    {
        foreach (var element in elements ?? throw new ArgumentException("Elements cannot be null.", nameof(elements)))
        {
            bucket[element] = false;
        }
    }

    public void Add(TKey element)
    {
        bucket[element] = true;
    }

    public bool this[TKey element] => bucket[element]!;

    public bool IsFull => bucket.Values.All(v => v);

    public override string ToString()
    {
        return string.Join(' ', bucket.Select(kvp => $"{kvp.Key}:{(kvp.Value ? 1 : 0)}"));
    }
}
