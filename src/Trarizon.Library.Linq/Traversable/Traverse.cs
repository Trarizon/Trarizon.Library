using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static IEnumerable<T> TraverseDeepFirst<T>(this IChildrenTraversable<T> source) where T : IChildrenTraversable<T>
    {
        using var traverser = new DeepFirstTraverser<T>(source.GetChildrenEnumerator());
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseBreadthFirst<T>(this IChildrenTraversable<T> source) where T : IChildrenTraversable<T>
    {
        using var traverse = new BreadthFirstTraverser<T>(source.GetChildrenEnumerator());
        while (traverse.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseDeepFirst<T>(this T source, bool includeSelf = false) where T : IChildrenTraversable<T>
    {
        if (includeSelf)
            yield return source;

        using var traverser = new DeepFirstTraverser<T>(source.GetChildrenEnumerator());
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseBreadthFirst<T>(this T source, bool includeSelf = false) where T : IChildrenTraversable<T>
    {
        if (includeSelf)
            yield return source;

        using var traverse = new BreadthFirstTraverser<T>(source.GetChildrenEnumerator());
        while (traverse.MoveNext(out var current)) {
            yield return current;
        }
    }

    private readonly struct DeepFirstTraverser<T> : IDisposable
        where T : IChildrenTraversable<T>
    {
        private readonly Stack<IEnumerator<T>> _stack;

        internal DeepFirstTraverser(IEnumerator<T> root)
        {
            _stack = new();
            _stack.Push(root);
        }

        public bool MoveNext([MaybeNullWhen(false)] out T current)
        {
            while (_stack.TryPeek(out var enumerator)) {
                if (enumerator.MoveNext()) {
                    current = enumerator.Current;
                    _stack.Push(current.GetChildrenEnumerator());
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

    private struct BreadthFirstTraverser<T> : IDisposable
        where T : IChildrenTraversable<T>
    {
        private readonly Queue<IEnumerator<T>> _queue;
        private IEnumerator<T> _enumerator;

        internal BreadthFirstTraverser(IEnumerator<T> root)
        {
            _queue = new();
            _enumerator = root;
        }

        public bool MoveNext([MaybeNullWhen(false)] out T current)
        {
        Begin:
            if (_enumerator.MoveNext()) {
                current = _enumerator.Current;
                _queue.Enqueue(current.GetChildrenEnumerator());
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
}
