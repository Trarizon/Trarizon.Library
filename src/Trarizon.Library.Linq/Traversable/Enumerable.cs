using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Linq.Internal;

namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static IEnumerable<T> TraverseDescendantsDepthFirst<T>(this IEnumerable<T> firstLevel, Func<T, IEnumerable<T>?> childrenSelector)
    {
        using var traverser = new DeepFirstTraverser<T, DelegateChildrenEnumeratorProvider<T>>(
            firstLevel.GetEnumerator(), new(childrenSelector));
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseDescendantsBreadthFirst<T>(this IEnumerable<T> firstLevel, Func<T, IEnumerable<T>> childrenSelector)
    {
        using var traverse = new DeepFirstTraverser<T, DelegateChildrenEnumeratorProvider<T>>(
            firstLevel.GetEnumerator(), new(childrenSelector));
        while (traverse.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseDescendantsDepthFirst<T>(T self, Func<T, IEnumerable<T>?> childrenSelector, bool includeSelf)
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

    public static IEnumerable<T> TraverseDescendantsBreadthFirst<T>(T self, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf)
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


    private readonly struct DeepFirstTraverser<T, TChildrenEnumeratorProvider> : IDisposable
        where TChildrenEnumeratorProvider : IChildrenEnumeratorProvider<T>
    {
        private readonly Stack<IEnumerator<T>> _stack;
        private readonly TChildrenEnumeratorProvider _provider;

        internal DeepFirstTraverser(IEnumerator<T> root, TChildrenEnumeratorProvider provider)
        {
            _stack = new();
            _stack.Push(root);
            _provider = provider;
        }

        public bool MoveNext([MaybeNullWhen(false)] out T current)
        {
            while (_stack.TryPeek(out var enumerator)) {
                if (enumerator.MoveNext()) {
                    current = enumerator.Current;
                    _stack.Push(_provider.GetChildrenEnumerator(current));
                    return true;
                }
                else {
                    enumerator.Dispose();
                    _stack.Pop();
                }
            }
            current = default;
            return false;
        }

        public void Dispose()
        {
            foreach (var item in _stack) {
                item.Dispose();
            }
        }
    }

    private struct BreadthFirstTraverser<T, TChildrenEnumeratorProvider> : IDisposable
        where TChildrenEnumeratorProvider : IChildrenEnumeratorProvider<T>
    {
        private readonly Queue<IEnumerator<T>> _queue;
        private IEnumerator<T> _enumerator;
        private readonly TChildrenEnumeratorProvider _provider;

        internal BreadthFirstTraverser(IEnumerator<T> root, TChildrenEnumeratorProvider provider)
        {
            _queue = new();
            _enumerator = root;
            _provider = provider;
        }

        public bool MoveNext([MaybeNullWhen(false)] out T current)
        {
        Begin:
            if (_enumerator.MoveNext()) {
                current = _enumerator.Current;
                _queue.Enqueue(_provider.GetChildrenEnumerator(current));
                return true;
            }

            _enumerator.Dispose();

            if (_queue.TryDequeue(out var en)) {
                _enumerator = en;
                goto Begin;
            }

            current = default;
            return false;
        }
        public void Dispose()
        {
            _enumerator.Dispose();
            foreach (var item in _queue) {
                item.Dispose();
            }
        }
    }

    private readonly struct DelegateChildrenEnumeratorProvider<T>(Func<T, IEnumerable<T>?> func) : IChildrenEnumeratorProvider<T>
    {
        public IEnumerator<T> GetChildrenEnumerator(T source) => func(source)?.GetEnumerator() ?? TraEnumerable.EmptyEnumerator<T>();
    }
}
