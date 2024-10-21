using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static bool TryLast<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
    {
        if (source.TryGetNonEnumeratedCount(out var count)) {
            if (count > 0) {
                if (source is IList<T> list) {
                    value = list[^1];
                    return true;
                }
                goto ByEnumerate;
            }
            else {
                value = default;
                return false;
            }
        }

    ByEnumerate:

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            value = default;
            return false;
        }

        value = enumerator.Current;

        while (enumerator.MoveNext()) {
            value = enumerator.Current;
        }
        return true;
    }

    public static bool TryLast<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        using var enumerator = source.GetEnumerator();
        Optional<T> val = default;
        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                val = current;
            }
        }

        return val.TryGetValue(out value);
    }
}
