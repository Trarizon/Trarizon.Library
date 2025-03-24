namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
    public static IEnumerable<T> EnumerateDescendantsDepthFirst<T>(T self, Func<T, IEnumerable<T>?> childrenSelector)
    {
        var children = childrenSelector(self);
        if (children is null)
            yield break;

        var childrenEnumerator = children.GetEnumerator();

        Stack<IEnumerator<T>> enumerators = new();
        enumerators.Push(childrenEnumerator);

        try {
            while (enumerators.TryPeek(out var enumerator)) {
                if (enumerator.MoveNext()) {
                    var current = enumerator.Current;
                    yield return current;
                    var curChildren = childrenSelector(current);
                    if (curChildren is not null)
                        enumerators.Push(curChildren.GetEnumerator());
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

    public static IEnumerable<T> EnumerateDescendantsBreadthFirst<T>(T self, Func<T, IEnumerable<T>> childrenSelector)
    {
        Queue<T> queue = new();

        queue.Enqueue(self);

        while (queue.TryDequeue(out var current)) {
            foreach (var child in childrenSelector(current)) {
                yield return child;
                queue.Enqueue(child);
            }
        }
    }
}