using CommunityToolkit.HighPerformance;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) {
            action.Invoke(item);
        }
    }

    /// <summary>
    /// If <paramref name="source"/> is <see langword="null"/>, this method returns an empty collection,
    /// else return <paramref name="source"/> it self
    /// </summary>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
        => source ?? [];

    public static bool TryToNonEmptyList<T>(this IEnumerable<T> source, [NotNullWhen(true)] out List<T>? list)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count == 0) {
                list = null;
                return false;
            }
            else {
                list = source.ToList();
                return true;
            }
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            list = null;
            return false;
        }

        list = [enumerator.Current];
        while (enumerator.MoveNext()) {
            list.Add(enumerator.Current);
        }
        return true;
    }

    public static bool TryGetSpan<T>(this IEnumerable<T> source, out ReadOnlySpan<T> span)
    {
        if (source is T[] array) {
            span = array.AsSpan();
            return true;
        }
        if (source is List<T> list) {
            span = list.AsSpan();
            return true;
        }
        span = default;
        return false;
    }
}
