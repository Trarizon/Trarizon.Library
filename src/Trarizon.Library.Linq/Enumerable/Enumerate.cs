namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until <paramref name="predicate"/> returns false.
    /// </summary>
    public static IEnumerable<T> EnumerateByWhile<T>(T first, Func<T, T> nextSelector, Func<T, bool> predicate)
    {
        var val = first;
        while (predicate(val)) {
            yield return val;
            val = nextSelector(val);
        }
    }

    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until current value is null.
    /// </summary>
    public static IEnumerable<T> EnumerateByNotNull<T>(T? first, Func<T, T?> nextSelector) where T : class
    {
        var val = first;
        while (val is not null) {
            yield return val;
            val = nextSelector(val);
        }
    }

    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until current value is null.
    /// </summary>
    public static IEnumerable<T> EnumerateByNotNull<T>(T? first, Func<T, T?> nextSelector) where T : struct
    {
        var nval = first;
        while (nval is { } val) {
            yield return val;
            nval = nextSelector(val);
        }
    }

    /// <summary>
    /// Traverse tree nodes
    /// </summary>
    public static IEnumerable<T> EnumerateDescendants<T>(T self, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf = false, bool depthFirst = false)
    {
        if (depthFirst)
            return EnumerateDescendantsDepthFirst(self, childrenSelector, includeSelf);
        else
            return EnumerateDescendantsBreadthFirst(self, childrenSelector, includeSelf);
    }

    private static IEnumerable<T> EnumerateDescendantsDepthFirst<T>(T self, Func<T, IEnumerable<T>?> childrenSelector, bool includeSelf)
    {
        if (includeSelf)
            yield return self;

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

    private static IEnumerable<T> EnumerateDescendantsBreadthFirst<T>(T self, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf)
    {
        if (includeSelf)
            yield return self;

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
