using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Linq.Internal;

namespace Trarizon.Library.Linq;

public static partial class TraTraversable
{
    public static IEnumerable<T> TraverseDepthFirst<T>(this IChildrenTraversable<T> source) where T : IChildrenTraversable<T>
    {
        using var traverser = new DepthFirstTraverser<T, ChildrenTraversableEnumeratorProvider<T>>(source.GetChildrenEnumerator(), default);
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseBreadthFirst<T>(this IChildrenTraversable<T> source) where T : IChildrenTraversable<T>
    {
        using var traverser = new BreadthFirstTraverser<T, ChildrenTraversableEnumeratorProvider<T>>(source.GetChildrenEnumerator(), default);
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseDepthFirst<T>(this T source, bool includeSelf = false) where T : IChildrenTraversable<T>
    {
        if (includeSelf)
            yield return source;

        using var traverser = new DepthFirstTraverser<T, ChildrenTraversableEnumeratorProvider<T>>(source.GetChildrenEnumerator(), default);
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    public static IEnumerable<T> TraverseBreadthFirst<T>(this T source, bool includeSelf = false) where T : IChildrenTraversable<T>
    {
        if (includeSelf)
            yield return source;

        using var traverser = new BreadthFirstTraverser<T, ChildrenTraversableEnumeratorProvider<T>>(source.GetChildrenEnumerator(), default);
        while (traverser.MoveNext(out var current)) {
            yield return current;
        }
    }

    private readonly struct DepthFirstTraverser<T, TChildrenEnumeratorProvider> : IDisposable
        where TChildrenEnumeratorProvider : IChildrenEnumeratorProvider<T>
    {
        private readonly Stack<IEnumerator<T>> _stack;
        private readonly TChildrenEnumeratorProvider _provider;

        internal DepthFirstTraverser(IEnumerator<T> root, TChildrenEnumeratorProvider provider)
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

    private readonly struct ChildrenTraversableEnumeratorProvider<T> : IChildrenEnumeratorProvider<T>
        where T : IChildrenTraversable<T>
    {
        public IEnumerator<T> GetChildrenEnumerator(T source) => source.GetChildrenEnumerator();
    }
}
