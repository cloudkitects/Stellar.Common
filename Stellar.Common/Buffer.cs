using Stellar.Common.Resources;
using System.Diagnostics.CodeAnalysis;

namespace Stellar.Common;

/// <summary>
/// A generic buffer filled by a callback.
/// </summary>
 [ExcludeFromCodeCoverage(Justification = "Fully tested by Stellar.IO")]
 public class Buffer<T>
{
    private readonly Func<T[], int, int>? fillCallback;

    public T[] Items { get; }

    public T this[int index] => Items[index];

    public T Current => Items[Position];


    public int Capacity => Items.Length;

    public int Count { get; private set; }

    public int Position { get; set; }

    public IDictionary<string, int> Bookmarks { get; }

    public Buffer(int capacity, Func<T[], int, int>? fillCallback = null, IEqualityComparer<string>? bookmarkComparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);

        Items = new T[capacity];

        this.fillCallback = fillCallback;

        Bookmarks = new Dictionary<string, int>(bookmarkComparer);
    }

    public bool Refill() => Position < Count || Fill();

    public bool Fill(int keepCount = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keepCount);

        if (fillCallback is null)
        {
            return false;
        }

        if (keepCount == 0)
        {
            Position = 0;
        }
        else
        {
            if (keepCount >= Items.Length)
            {
                throw new ArgumentException(Exceptions.CannotKeepAllBufferElements, nameof(keepCount));
            }

            Array.Copy(Items, Count - keepCount, Items, 0, keepCount);

            Position = keepCount - (Count - Position);

            foreach (var key in Bookmarks.Keys.ToArray())
            {
                Bookmarks[key] = keepCount - (Count - Bookmarks[key]);
            }
        }

        var readLength = fillCallback(Items, keepCount);

        Count = readLength + keepCount;

        return readLength > 0;
    }

    public void Clear(bool purgeData = false)
    {
        Position = 0;
        Count = 0;

        if (purgeData)
        {
            Array.Clear(Items, 0, Items.Length);
        }
    }
}
