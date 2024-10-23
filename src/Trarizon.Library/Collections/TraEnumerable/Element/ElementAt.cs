using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Generic;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static bool TryAt<T>(this IEnumerable<T> source, int index, [MaybeNullWhen(false)] out T element)
    {
        if (index < 0) {
            element = default;
            return false;
        }

        if (source is IList<T> list) {
            if (index >= list.Count) {
                element = default;
                return false;
            }
            else {
                element = list[index];
                return true;
            }
        }

        using var enumerator = source.GetEnumerator();
        if (enumerator.TryIterate(index, out _)) {
            element = enumerator.Current;
            return true;
        }
        else {
            element = default;
            return false;
        }
    }

    public static bool TryAt<T>(this IEnumerable<T> source, Index index, [MaybeNullWhen(false)] out T element)
    {
        if (!index.IsFromEnd) {
            return source.TryAt(index.Value, out element);
        }
        if (source.TryGetNonEnumeratedCount(out var count)) {
            return source.TryAt(count - index.Value, out element);
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            element = default;
            return false;
        }

        var queue = new RingQueue<T>(index.Value);
        do {
            queue.Enqueue(enumerator.Current);
        } while (enumerator.MoveNext());
        if (queue.IsFull) {
            element = queue.PeekFirst();
            return true;
        }

        element = default;
        return false;
    }
}
