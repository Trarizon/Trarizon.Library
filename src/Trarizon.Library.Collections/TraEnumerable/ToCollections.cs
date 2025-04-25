using CommunityToolkit.HighPerformance;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
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

    internal static bool TryGetSpan<T>(this IEnumerable<T> source, out ReadOnlySpan<T> span)
    {
        if (source.GetType() == typeof(T[])) {
            span = Unsafe.As<T[]>(source).AsSpan();
            return true;
        }
        if (source.GetType() == typeof(List<T>)) {
            span = Unsafe.As<List<T>>(source).AsSpan();
            return true;
        }
        span = default;
        return false;
    }
}
