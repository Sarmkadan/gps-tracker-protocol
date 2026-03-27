#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Collection extension methods for filtering, grouping, and aggregation.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Chunks a sequence into batches of specified size.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be greater than 0");

        using (var enumerator = source.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                yield return ChunkIterator(enumerator, chunkSize - 1);
            }
        }
    }

    private static IEnumerable<T> ChunkIterator<T>(IEnumerator<T> enumerator, int remainingCount)
    {
        yield return enumerator.Current;

        while (remainingCount > 0 && enumerator.MoveNext())
        {
            yield return enumerator.Current;
            remainingCount--;
        }
    }

    /// <summary>
    /// Gets the median value from a numeric sequence.
    /// </summary>
    public static double Median<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        var sorted = source.Select(selector).OrderBy(x => x).ToList();

        if (sorted.Count == 0)
            return 0;

        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2
            : sorted[mid];
    }

    /// <summary>
    /// Removes duplicate items while preserving order.
    /// </summary>
    public static IEnumerable<T> DistinctByOrder<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seen.Add(key))
                yield return item;
        }
    }

    /// <summary>
    /// Finds the minimum and maximum values in a sequence.
    /// </summary>
    public static (T min, T max) MinMax<T>(this IEnumerable<T> source) where T : IComparable<T>
    {
        var items = source.ToList();
        if (items.Count == 0)
            throw new InvalidOperationException("Sequence is empty");

        var min = items[0];
        var max = items[0];

        foreach (var item in items.Skip(1))
        {
            if (item.CompareTo(min) < 0)
                min = item;
            if (item.CompareTo(max) > 0)
                max = item;
        }

        return (min, max);
    }

    /// <summary>
    /// Calculates the percentage of items matching a condition.
    /// </summary>
    public static double PercentageWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var items = source.ToList();
        if (items.Count == 0)
            return 0;

        var matching = items.Count(predicate);
        return (matching / (double)items.Count) * 100;
    }

    /// <summary>
    /// Safely indexes a list with bounds checking.
    /// </summary>
    public static T SafeGet<T>(this IList<T> source, int index, T defaultValue = default)
    {
        return index >= 0 && index < source.Count ? source[index] : defaultValue;
    }

    /// <summary>
    /// Gets items in a sliding window fashion.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> SlidingWindow<T>(this IEnumerable<T> source, int windowSize)
    {
        if (windowSize <= 0)
            throw new ArgumentException("Window size must be greater than 0");

        var items = source.ToList();
        for (int i = 0; i <= items.Count - windowSize; i++)
        {
            yield return items.Skip(i).Take(windowSize);
        }
    }
}
