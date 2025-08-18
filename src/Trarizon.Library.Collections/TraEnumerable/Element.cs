using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static bool TryAt<T>(this IEnumerable<T> source, int index, [MaybeNullWhen(false)] out T element)
    {
        if (index < 0) {
            element = default;
            return false;
        }

        if (source is IteratorBase<T> iterator) {
            element = iterator.TryCheapAt(index, out var exists);
            return exists;
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

        var queue = new Queue<T>(index.Value);
        do {
            if (queue.Count == index.Value)
                queue.Dequeue();
            queue.Enqueue(enumerator.Current);
        } while (enumerator.MoveNext());

        if (queue.Count == index.Value) {
            element = queue.Peek();
            return true;
        }

        element = default;
        return false;
    }
}
