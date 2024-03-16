using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    public static bool TryFirst<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
        => source.TryFirst(out value, default!);

    public static bool TryFirst<T>(this IEnumerable<T> source, out T value, T defaultValue)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count > 0) {
                if (source is IList<T> list) {
                    value = list[0];
                    return true;
                }
                goto ByEnumerate;
            }
            else {
                value = defaultValue;
                return false;
            }
        }

    ByEnumerate:

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            return true;
        }
        else {
            value = defaultValue;
            return false;
        }
    }

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
        => source.TryFirst(predicate, out value, default!);

    public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T value, T defaultValue)
    {
        using var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                value = current;
                return true;
            }
        }
        value = defaultValue;
        return false;
    }
}
