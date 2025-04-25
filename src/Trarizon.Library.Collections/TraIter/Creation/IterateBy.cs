using System.ComponentModel;

namespace Trarizon.Library.Collections;
public static partial class TraIter
{
    /// <summary>
    /// Struct version of <see cref="TraEnumerable.EnumerateByWhile{T}(T, Func{T, T}, Func{T, bool})"/>
    /// <br/>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until <paramref name="predicate"/> returns false.
    /// </summary>
    public static IterateByIterator<T> IterateByWhile<T>(T first, Func<T, T> nextSelector, Func<T, bool> predicate)
        => new(first, nextSelector, predicate);

    /// <summary>
    /// Struct version of <see cref="TraEnumerable.EnumerateByNotNull{T}(T?, Func{T, T?})"/>
    /// <br/>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until current value is null.
    /// </summary>
    public static IterateByClassNotNullIterator<T> IterateByNotNull<T>(T? first, Func<T, T?> nextSelector) where T : class
        => new(first, nextSelector);

    /// <summary>
    /// Struct version of <see cref="TraEnumerable.EnumerateByNotNull{T}(T?, Func{T, T?})"/>
    /// <br/>
    /// yield <paramref name="first"/>, and then
    /// repeatly call <paramref name="nextSelector"/> to create next value.
    /// Enumerate stop until current value is null.
    /// </summary>
    public static IterateByStructNotNullIterator<T> IterateByNotNull<T>(T? first, Func<T, T?> nextSelector) where T : struct
        => new(first, nextSelector);


    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct IterateByIterator<T>(T first, Func<T, T> nextSelector, Func<T, bool> predicate)
    {
        private int _state;
        private T _current = default!;

        public IterateByIterator<T> GetEnumerator() => this with { _state = -1 };

        public readonly T Current => _current;

        public bool MoveNext()
        {
            if (_state == 0) {
                return false;
            }
            else if (_state == -1) {
                if (predicate(first)) {
                    _current = first;
                    _state = 1;
                    return true;
                }
                else {
                    _state = 0;
                    return false;
                }
            }
            else {
                var cur = nextSelector(_current);
                if (predicate(cur)) {
                    _current = cur;
                    return true;
                }
                else {
                    _state = 0;
                    return false;
                }
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct IterateByClassNotNullIterator<T>(T? first, Func<T, T?> nextSelector)
        where T : class
    {
        private int _state;
        private T _current = default!;

        public IterateByClassNotNullIterator<T> GetEnumerator() => this with { _state = -1 };

        public readonly T Current => _current;

        public bool MoveNext()
        {
            if (_state == 0) {
                return false;
            }
            else if (_state == -1) {
                if (first is not null) {
                    _current = first;
                    _state = 1;
                    return true;
                }
                else {
                    _state = 0;
                    return false;
                }
            }
            else {
                var cur = nextSelector(_current);
                if (cur is not null) {
                    _current = cur;
                    return true;
                }
                else {
                    _state = 0;
                    return false;
                }
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct IterateByStructNotNullIterator<T>(T? first, Func<T, T?> nextSelector)
        where T : struct
    {
        private int _state;
        private T _current;
        public IterateByStructNotNullIterator<T> GetEnumerator() => this with { _state = -1 };

        public readonly T Current => _current;

        public bool MoveNext()
        {
            if (_state == 0) {
                return false;
            }
            else if (_state == -1) {
                if (first is not null) {
                    _current = first.Value;
                    _state = 1;
                    return true;
                }
                else {
                    _state = 0;
                    return false;
                }
            }
            else {
                var cur = nextSelector(_current);
                if (cur is not null) {
                    _current = cur.Value;
                    return true;
                }
                else {
                    _state = 0;
                    return false;
                }
            }
        }
    }
}
