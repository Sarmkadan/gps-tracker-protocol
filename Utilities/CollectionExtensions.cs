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
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to chunk.</param>
    /// <param name="chunkSize">The size of each chunk. Must be greater than 0.</param>
    /// <returns>An enumerable of chunks, each containing up to <paramref name="chunkSize"/> elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkSize"/> is less than or equal to 0.</exception>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than 0");

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
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to calculate median from.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>The median value of the sequence, or 0 if the sequence is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    public static double Median<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

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
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of key used for comparison.</typeparam>
    /// <param name="source">The source sequence to filter.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An enumerable containing only the first occurrence of each distinct key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> DistinctByOrder<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

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
    /// <typeparam name="T">The type of elements in the sequence. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="source">The source sequence to search.</param>
    /// <returns>A tuple containing the minimum and maximum values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
    public static (T min, T max) MinMax<T>(this IEnumerable<T> source) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(source);

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
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to evaluate.</param>
    /// <param name="predicate">The function to test each element.</param>
    /// <returns>The percentage of items that match the predicate (0-100).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static double PercentageWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var items = source.ToList();
        if (items.Count == 0)
            return 0;

        var matching = items.Count(predicate);
        return (matching / (double)items.Count) * 100;
    }

    /// <summary>
    /// Safely indexes a list with bounds checking.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="source">The list to index.</param>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <param name="defaultValue">The value to return if the index is out of bounds.</param>
    /// <returns>The element at the specified index, or <paramref name="defaultValue"/> if the index is invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static T SafeGet<T>(this IList<T> source, int index, T defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        return index >= 0 && index < source.Count ? source[index] : defaultValue;
    }

    /// <summary>
    /// Gets items in a sliding window fashion.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence to window.</param>
    /// <param name="windowSize">The size of each sliding window. Must be greater than 0.</param>
    /// <returns>An enumerable of windows, each containing <paramref name="windowSize"/> consecutive elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="windowSize"/> is less than or equal to 0.</exception>
    public static IEnumerable<IEnumerable<T>> SlidingWindow<T>(this IEnumerable<T> source, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (windowSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size must be greater than 0");

        var items = source.ToList();
        if (items.Count == 0)
            yield break;

        for (int i = 0; i <= items.Count - windowSize; i++)
        {
            yield return items.Skip(i).Take(windowSize);
        }
    }
}
