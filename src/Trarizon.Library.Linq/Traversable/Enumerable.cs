using Trarizon.Library.Linq.Internal;

namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static IEnumerable<T> TraverseDescendantsDepthFirst<T>(this IEnumerable<T> firstLevel, Func<T, IEnumerable<T>?> childrenSelector)
    {
        using var traverser = new DepthFirstTraverser<T, DelegateChildrenEnumeratorProvider<T>>(
            firstLevel.GetEnumerator(), new(childrenSelector));
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseDescendantsBreadthFirst<T>(this IEnumerable<T> firstLevel, Func<T, IEnumerable<T>> childrenSelector)
    {
        using var traverser = new BreadthFirstTraverser<T, DelegateChildrenEnumeratorProvider<T>>(
            firstLevel.GetEnumerator(), new(childrenSelector));
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseDescendantsDepthFirst<T>(T self, Func<T, IEnumerable<T>?> childrenSelector, bool includeSelf = false)
    {
        if (includeSelf)
            yield return self;

        var children = childrenSelector(self);
        if (children is null)
            yield break;

        using var traverser = new DepthFirstTraverser<T, DelegateChildrenEnumeratorProvider<T>>(
            children.GetEnumerator(), new(childrenSelector));
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseDescendantsBreadthFirst<T>(T self, Func<T, IEnumerable<T>?> childrenSelector, bool includeSelf = false)
    {
        if (includeSelf)
            yield return self;

        var children = childrenSelector(self);
        if (children is null)
            yield break;

        using var traverse = new BreadthFirstTraverser<T, DelegateChildrenEnumeratorProvider<T>>(
            children.GetEnumerator(), new(childrenSelector));
        while (traverse.MoveNext(out var current)) {
            yield return current;
        }
    }

    private readonly struct DelegateChildrenEnumeratorProvider<T>(Func<T, IEnumerable<T>?> func) : IChildrenEnumeratorProvider<T>
    {
        public IEnumerator<T> GetChildrenEnumerator(T source) => func(source)?.GetEnumerator() ?? TraEnumerable.EmptyEnumerator<T>();
    }
}
