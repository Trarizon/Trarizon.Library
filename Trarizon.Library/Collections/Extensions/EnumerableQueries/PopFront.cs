using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Extensions.Helpers.Queriers;

namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    /// <summary>
    /// Pop specific number of elements, and return the rest,
    /// popped elements are cached in <paramref name="leadingElements"/>
    /// </summary>
    public static IEnumerable<T> PopFront<T>(this IEnumerable<T> source, int count, out IList<T> leadingElements)
    {
        if (source is IList<T> list) {
            return list.PopFrontList(count, out leadingElements);
        }

        if (count <= 0) {
            leadingElements = Array.Empty<T>();
            return source;
        }

        var enumerator = source.GetEnumerator();
        var array = new T[count];
        var len = 0;
        while (len < count && enumerator.MoveNext()) {
            array[len] = enumerator.Current;
            len++;
        }

        if (len < count) {
            leadingElements = array.TakeList(..len);
            return Enumerable.Empty<T>();
        }
        else {
            leadingElements = array;
            return new PopFrontQuerier<T>(source, enumerator, count);
        }
    }

    /// <summary>
    /// Pop first element and return the rest,
    /// first element is <paramref name="firstElement"/>.
    /// If no element in <paramref name="source"/>,
    /// <paramref name="firstElement"/> is <paramref name="defaultValue"/>
    /// </summary>
    /// <remarks>
    /// About nullable analysis:
    /// There's no warning if <typeparamref name="T"/> is nullable reference type.
    /// More remarks at <see cref="TrySingle"/>
    /// </remarks>
    public static IEnumerable<T> PopFirst<T>(this IEnumerable<T> source, [NotNullIfNotNull(nameof(defaultValue))] out T? firstElement, T? defaultValue = default!)
    {
        if (source is IList<T> list) {
            return list.PopFirstList(out firstElement, defaultValue);
        }

        var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext()) {
            firstElement = enumerator.Current;
            return new PopFrontQuerier<T>(source, enumerator, 1);
        }
        else {
            firstElement = defaultValue;
            return Enumerable.Empty<T>();
        }
    }

    /// <summary>
    /// Pop elements until <paramref name="predicate"/> failed.
    /// popped elements are cached in <paramref name="leadingElements"/>
    /// </summary>
    public static IEnumerable<T> PopFrontWhile<T>(this IEnumerable<T> source, out IList<T> leadingElements, Func<T, bool> predicate)
    {
        if (source is IList<T> list) {
            return list.PopFrontWhileList(out leadingElements, predicate);
        }

        var enumerator = source.GetEnumerator();
        List<T> tmpList = new();
        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(enumerator.Current)) {
                tmpList.Add(current);
            }
            else {
                leadingElements = tmpList;
                return new PopFrontWhileQuerier<T>(source, enumerator, current, tmpList.Count);
            }
        }
        leadingElements = tmpList;
        return Enumerable.Empty<T>();
    }


    private sealed class PopFrontQuerier<T> : SimpleEnumerationQuerier<T, T>
    {
        private readonly int _skipCount;

        public PopFrontQuerier(IEnumerable<T> source, IEnumerator<T> enumerator, int skippedCount) : base(source)
        {
            _enumerator = enumerator;
            _skipCount = skippedCount;
        }

        public override bool MoveNext()
        {
            const int Iterating = MinPreservedState - 1;
            const int Ended = Iterating - 1;

            switch (_state) {
                case -1:
                    if (_enumerator == null) {
                        _enumerator = _source.GetEnumerator();
                        _enumerator.TryIterate(_skipCount, out var iterated);
                        // No element rest
                        if (iterated < _skipCount) {
                            _state = Ended;
                            return false;
                        }
                    }
                    _state = Iterating;
                    goto case Iterating;

                case Iterating:
                    if (_enumerator!.MoveNext()) {
                        _current = _enumerator.Current;
                        return true;
                    }
                    else {
                        _state = Ended;
                        return false;
                    }

                case Ended:
                default:
                    return false;
            }
        }

        protected override EnumerationQuerier<T> Clone() => new PopFrontQuerier<T>(_source, null!, _skipCount);
    }

    private sealed class PopFrontWhileQuerier<T> : SimpleEnumerationQuerier<T, T>
    {
        private readonly int _skipCount;
        private readonly T _first;

        public PopFrontWhileQuerier(IEnumerable<T> source, IEnumerator<T> enumerator, T first, int skippedCount) : base(source)
        {
            _enumerator = enumerator;
            _first = first;
            _skipCount = skippedCount + 1; // skip <param first>
        }

        private PopFrontWhileQuerier(IEnumerable<T> source, T first, int skipCount) : base(source)
        {
            _first = first;
            _skipCount = skipCount;
        }

        public override bool MoveNext()
        {
            const int AfterFirst = MinPreservedState - 1;
            const int Iterating = AfterFirst - 1;
            const int Ended = Iterating - 1;

            switch (_state) {
                case -1:
                    _current = _first;
                    _state = AfterFirst;
                    return true;
                case AfterFirst:
                    if (_enumerator == null) {
                        _enumerator = _source.GetEnumerator();
                        _enumerator.TryIterate(_skipCount, out var iterated);
                        // No element rest
                        if (iterated < _skipCount) {
                            _state = Ended;
                            return false;
                        }
                    }
                    _state = Iterating;
                    goto case Iterating;

                case Iterating:
                    if (_enumerator!.MoveNext()) {
                        _current = _enumerator.Current;
                        return true;
                    }
                    else {
                        _state = Ended;
                        return false;
                    }

                case Ended:
                default:
                    return false;
            }
        }

        protected override EnumerationQuerier<T> Clone() => new PopFrontWhileQuerier<T>(_source, _first, _skipCount);
    }
}
