using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    #region EnumerateByWhile

    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextWhereSelector"/> to create next value.
    /// Enumerate stop until <paramref name="nextWhereSelector"/> returns <see cref="Optional{T}.None"/>
    /// </summary>
    public static EnumerateByWhileOptionalEnumerable<T> EnumerateByWhile<T>(T first, Func<T, Optional<T>> nextWhereSelector)
        => new(first, nextWhereSelector);

    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until <paramref name="predicate"/> returns false.
    /// </summary>
    public static EnumerateByWhilePredicateEnumerable<T> EnumerateByWhile<T>(T first, Func<T, T> nextSelector, Func<T, bool> predicate)
        => new(first, nextSelector, predicate);

    /// <summary>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until current value is null.
    /// </summary>
    public static EnumerateByWhileNotNullEnumerable<T> EnumerateByWhileNotNull<T>(T? first, Func<T, T?> nextWhereSeletor) where T : class
        => new(first, nextWhereSeletor);

    #region Structs

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct EnumerateByWhilePredicateEnumerable<T>(T first, Func<T, T> nextSelector, Func<T, bool> predicate) : IEnumerable<T>, IEnumerateByWhileSelector<T>
    {
        private readonly T _first = first;

        public SelectNextByWhileEnumerator<T, EnumerateByWhilePredicateEnumerable<T>> GetEnumerator() => new(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new SelectNextByWhileEnumerator<T, EnumerateByWhilePredicateEnumerable<T>>.Wrapper(this);
        IEnumerator IEnumerable.GetEnumerator() => new SelectNextByWhileEnumerator<T, EnumerateByWhilePredicateEnumerable<T>>.Wrapper(this);

        bool IEnumerateByWhileSelector<T>.TryGetFirst([MaybeNullWhen(false)] out T first)
        {
            if (predicate(_first)) {
                first = _first;
                return true;
            }
            else {
                first = default;
                return false;
            }
        }
        bool IEnumerateByWhileSelector<T>.TryGetNext(T prev, [MaybeNullWhen(false)] out T next)
        {
            var mayNext = nextSelector(prev);
            if (predicate(mayNext)) {
                next = mayNext;
                return true;
            }
            else {
                next = default;
                return false;
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct EnumerateByWhileOptionalEnumerable<T>(T first, Func<T, Optional<T>> whereSelector) : IEnumerable<T>, IEnumerateByWhileSelector<T>
    {
        private readonly T _first = first;

        public SelectNextByWhileEnumerator<T, EnumerateByWhileOptionalEnumerable<T>> GetEnumerator() => new(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new SelectNextByWhileEnumerator<T, EnumerateByWhileOptionalEnumerable<T>>.Wrapper(this);
        IEnumerator IEnumerable.GetEnumerator() => new SelectNextByWhileEnumerator<T, EnumerateByWhileOptionalEnumerable<T>>.Wrapper(this);

        bool IEnumerateByWhileSelector<T>.TryGetFirst([MaybeNullWhen(false)] out T first)
        {
            first = _first;
            return true;
        }
        bool IEnumerateByWhileSelector<T>.TryGetNext(T prev, [MaybeNullWhen(false)] out T next)
            => whereSelector(prev).TryGetValue(out next);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct EnumerateByWhileNotNullEnumerable<T>(T? first, Func<T, T?> whereSelector) : IEnumerable<T>, IEnumerateByWhileSelector<T> where T : class
    {
        private readonly T? _first = first;

        public SelectNextByWhileEnumerator<T, EnumerateByWhileNotNullEnumerable<T>> GetEnumerator() => new(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new SelectNextByWhileEnumerator<T, EnumerateByWhileNotNullEnumerable<T>>.Wrapper(this);
        IEnumerator IEnumerable.GetEnumerator() => new SelectNextByWhileEnumerator<T, EnumerateByWhileNotNullEnumerable<T>>.Wrapper(this);

        bool IEnumerateByWhileSelector<T>.TryGetFirst([MaybeNullWhen(false)] out T first)
        {
            first = _first;
            return first is not null;
        }
        bool IEnumerateByWhileSelector<T>.TryGetNext(T prev, [MaybeNullWhen(false)] out T next)
        {
            next = whereSelector(prev);
            return next is not null;
        }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IEnumerateByWhileSelector<T>
    {
        bool TryGetFirst([MaybeNullWhen(false)] out T first);
        bool TryGetNext(T prev, [MaybeNullWhen(false)] out T next);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct SelectNextByWhileEnumerator<T, TSelector> where TSelector : IEnumerateByWhileSelector<T>
    {
        private int _state = 0;
        private T? _current;
        private readonly TSelector _selector;

        internal SelectNextByWhileEnumerator(TSelector selector) => _selector = selector;

        public readonly T Current => _current!;

        public bool MoveNext()
        {
            switch (_state) {
                case 0:
                    if (_selector.TryGetFirst(out _current)) {
                        _state = 1;
                        return true;
                    }
                    return true;
                case 1:
                    if (_selector.TryGetNext(_current!, out _current)) {
                        return true;
                    }
                    else {
                        _state = 2;
                        return false;
                    }
                default:
                    return false;
            }
        }

        internal sealed class Wrapper(TSelector selector) : IEnumerator<T>
        {
            private SelectNextByWhileEnumerator<T, TSelector> _enumerator = new(selector);

            public T Current => _enumerator.Current;

            public void Dispose() { }
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator._state = 0;

            object IEnumerator.Current => Current!;
        }
    }

    #endregion

    #endregion

    [Experimental(ExperimentalDiagnosticIds.EnumerableHelper_Continue)]
    public static IEnumerable<T> Continue<T>(IEnumerator<T> enumerator)
    {
        try {
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        } finally {
            enumerator.Dispose();
        }
    }
}