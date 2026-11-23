using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T, TKey, TTieBreaker>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        Func<T, TTieBreaker> tieBreakerSelector,
        int count,
        bool descending = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(tieBreakerSelector);

        if (count <= 0)
            return [];

        IOrderedEnumerable<T> ordered = descending
            ? source.OrderByDescending(keySelector)
            : source.OrderBy(keySelector);

        return ordered
            .ThenBy(tieBreakerSelector)
            .Take(count);
    }
}