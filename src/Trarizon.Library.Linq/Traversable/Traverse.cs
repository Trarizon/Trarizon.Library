namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static IEnumerable<T> DeepFirstTraverse<T>(this T source, bool includeSelf = false) where T : IChildrenTraversable<T>
    {
        if (includeSelf)
            yield return source;

        var childrenEnumerator = source.GetChildrenEnumerator();

        Stack<IEnumerator<T>> enumerators = new();
        enumerators.Push(childrenEnumerator);

        try {
            while (enumerators.TryPeek(out var enumerator)) {
                if (enumerator.MoveNext()) {
                    var current = enumerator.Current;
                    yield return current;
                    enumerators.Push(current.GetChildrenEnumerator());
                }
                else {
                    enumerator.Dispose();
                    enumerators.Pop();
                }
            }
        }
        finally {
            foreach (var e in enumerators) {
                e.Dispose();
            }
        }
    }

    public static IEnumerable<T> BreadthFirstTraverse<T>(this T source, bool includeSelf = false) where T : IChildrenTraversable<T>
    {
        if (includeSelf)
            yield return source;

        Queue<IChildrenTraversable<T>> queue = new();
        queue.Enqueue(source);

        while (queue.TryDequeue(out var current)) {
            using var enumerator = current.GetChildrenEnumerator();
            while (enumerator.MoveNext()) {
                var child = enumerator.Current;
                yield return child;
                queue.Enqueue(child);
            }
        }
    }
}
