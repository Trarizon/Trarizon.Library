using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections;
public static partial class TraEnumerable
{
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

    public static IEnumerable<T> EnumerateDescendants<T>(T self, Func<T, Optional<T>> firstChildSelector, Func<T, Optional<T>> nextSiblingSelector, bool includeSelf = false, bool depthFirst = false)
    {
        if (depthFirst)
            return EnumerateDescendantsDepthFirst(self, firstChildSelector, nextSiblingSelector, includeSelf);
        else
            return EnumerateDescendantsBreadthFirst(self, firstChildSelector, nextSiblingSelector, includeSelf);
    }

    private static IEnumerable<T> EnumerateDescendantsDepthFirst<T>(T self, Func<T, Optional<T>> firstChildSelector, Func<T, Optional<T>> nextSiblingSelector, bool includeSelf)
    {
        if (includeSelf)
            yield return self;

        var first = firstChildSelector(self);
        if (!first.HasValue)
            yield break;
        var stack = new Stack<T>();
        stack.Push(first.Value);

        while (stack.TryPeek(out var peek)) {
            yield return peek;
            if (firstChildSelector(peek).TryGetValue(out var next)) {
                stack.Push(next);
                continue;
            }

            while (stack.TryPop(out var pop)) {
                if (nextSiblingSelector(pop).TryGetValue(out next)) {
                    stack.Push(next);
                    break;
                }
            }
        }
    }

    private static IEnumerable<T> EnumerateDescendantsBreadthFirst<T>(T self, Func<T, Optional<T>> firstChildSelector, Func<T, Optional<T>> nextSiblingSelector, bool includeSelf)
    {
        if (includeSelf)
            yield return self;

        var first = firstChildSelector(self);
        if (!first.TryGetValue(out var item))
            yield break;

        var queue = new Queue<T>();
        queue.Enqueue(item);

        while (nextSiblingSelector(item).TryGetValue(out item)) {
            queue.Enqueue(item);
        }

        while (queue.TryDequeue(out var peek)) {
            yield return peek;
            var child = firstChildSelector(peek);
            if (child.TryGetValue(out var c)) {
                queue.Enqueue(c);
                while (nextSiblingSelector(c).TryGetValue(out c)) {
                    queue.Enqueue(c);
                }
            }
        }
    }
}
