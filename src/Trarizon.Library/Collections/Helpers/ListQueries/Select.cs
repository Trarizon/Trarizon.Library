// Extends Enumerable.Select which
// does not implement IList<>
// https://source.dot.net/#System.Linq/System/Linq/Select.cs,c669c338f82e311e

using System.Diagnostics;
using Trarizon.Library.Collections.Helpers.Queriers;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IReadOnlyList<TResult> SelectList<T, TResult>(this IList<T> list, Func<T, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new SelectQuerier<IList<T>, T, TResult, Selector<T, TResult>>(list, new(selector));
    }

    public static IReadOnlyList<TResult> SelectROList<T, TResult>(this IReadOnlyList<T> list, Func<T, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new SelectQuerier<ListWrapper<T>, T, TResult, Selector<T, TResult>>(list.Wrap(), new(selector));
    }


    public static IReadOnlyList<TResult> SelectList<T, TResult>(this IList<T> list, Func<T, int, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new SelectQuerier<IList<T>, T, TResult, IndexedSelector<T, TResult>>(list, new(selector));
    }

    public static IReadOnlyList<TResult> SelectROList<T, TResult>(this IReadOnlyList<T> list, Func<T, int, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new SelectQuerier<ListWrapper<T>, T, TResult, IndexedSelector<T, TResult>>(list.Wrap(), new(selector));
    }


    public static IList<TResult> SelectCachedList<T, TResult>(this IList<T> list, Func<T, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new CachedSelectQuerier<IList<T>, T, TResult, Selector<T, TResult>>(list, new(selector));
    }

    public static IReadOnlyList<TResult> SelectCachedROList<T, TResult>(this IReadOnlyList<T> list, Func<T, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new CachedSelectQuerier<ListWrapper<T>, T, TResult, Selector<T, TResult>>(list.Wrap(), new(selector));
    }


    public static IList<TResult> SelectCachedList<T, TResult>(this IList<T> list, Func<T, int, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new CachedSelectQuerier<IList<T>, T, TResult, IndexedSelector<T, TResult>>(list, new(selector));
    }

    public static IReadOnlyList<TResult> SelectCachedROList<T, TResult>(this IReadOnlyList<T> list, Func<T, int, TResult> selector)
    {
        if (list.Count == 0)
            return Array.Empty<TResult>();
        return new CachedSelectQuerier<ListWrapper<T>, T, TResult, IndexedSelector<T, TResult>>(list.Wrap(), new(selector));
    }


    private sealed class SelectQuerier<TList, TIn, TOut, TSelector>(TList list, TSelector selector)
        : SimpleListQuerier<TList, TIn, TOut>(list)
        where TList : IList<TIn>
        where TSelector : ISelector<TIn, TOut>
    {
        public override TOut this[int index] => selector.Select(_list[index], index);

        public override int Count => _list.Count;

        protected override EnumerationQuerier<TOut> Clone() => new SelectQuerier<TList, TIn, TOut, TSelector>(_list, selector);
    }

    private sealed class CachedSelectQuerier<TList, TIn, TOut, TSelector>(TList list, TSelector selector)
        : SimpleListQuerier<TList, TIn, TOut>(list)
        where TList : IList<TIn>
        where TSelector : ISelector<TIn, TOut>
    {
        private TOut[] _cache = null!;
        private byte[]? _isCachedMarks;

        public override TOut this[int index]
        {
            get {
                var count = Count;
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);

                if (_isCachedMarks is null) {
                    _isCachedMarks = new byte[StackAllocBitArray.GetArrayLength(count)];
                    _cache = new TOut[count];
                }

                Debug.Assert(_cache is not null);
                var bits = new StackAllocBitArray(_isCachedMarks);
                if (bits[index])
                    return _cache[index];

                bits[index] = true;
                return _cache[index] = selector.Select(_list[index], index);
            }
        }

        public override int Count => _list.Count;

        protected override void SetAt(int index, TOut item)
        {
            var count = Count;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);

            if (_isCachedMarks is null) {
                _isCachedMarks = new byte[StackAllocBitArray.GetArrayLength(count)];
                _cache = new TOut[count];
            }

            Debug.Assert(_cache is not null);
            var bits = new StackAllocBitArray(_isCachedMarks);
            bits[index] = true;
            _cache[index] = item;
        }

        protected override EnumerationQuerier<TOut> Clone() => new CachedSelectQuerier<TList, TIn, TOut, TSelector>(_list, selector);
    }

    private interface ISelector<TIn, TOut>
    {
        TOut Select(TIn value, int index);
    }

    private readonly struct Selector<TIn, TOut>(Func<TIn, TOut> selector) : ISelector<TIn, TOut>
    {
        public TOut Select(TIn value, int index) => selector(value);
    }

    private readonly struct IndexedSelector<TIn, TOut>(Func<TIn, int, TOut> selector) : ISelector<TIn, TOut>
    {
        public TOut Select(TIn value, int index) => selector(value, index);
    }
}
