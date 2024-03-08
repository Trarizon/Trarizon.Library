// Extends Enumerable.Select which
// does not implement IList<>
// https://source.dot.net/#System.Linq/System/Linq/Select.cs,c669c338f82e311e

using System.Diagnostics;
using Trarizon.Library.Collections.Helpers.Utilities.Queriers;
using Trarizon.Library.Collections.Helpers.ListQueries.Helpers;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IList<TResult> SelectList<T, TResult>(this IList<T> list, Func<T, TResult> selector)
        => new SelectQuerier<ListWrapper<T>, T, TResult>(list.Wrap(), selector);

    public static IList<TResult> SelectCachedList<T, TResult>(this IList<T> list, Func<T, TResult> selector)
        => new CachedSelectQuerier<ListWrapper<T>, T, TResult>(list.Wrap(), selector);

    public static IReadOnlyList<TResult> SelectROList<T, TResult>(this IReadOnlyList<T> list, Func<T, TResult> selector)
        => new SelectQuerier<IReadOnlyList<T>, T, TResult>(list, selector);

    public static IReadOnlyList<TResult> SelectCachedROList<T, TResult>(this IReadOnlyList<T> list, Func<T, TResult> selector)
        => new CachedSelectQuerier<IReadOnlyList<T>, T, TResult>(list, selector);

    private sealed class SelectQuerier<TList, TIn, TOut>(TList list, Func<TIn, TOut> selector) : SimpleListQuerier<TList, TIn, TOut>(list) where TList : IReadOnlyList<TIn>
    {
        public override TOut this[int index] => selector(_list[index]);

        public override int Count => _list.Count;

        protected override EnumerationQuerier<TOut> Clone() => new SelectQuerier<TList, TIn, TOut>(_list, selector);
    }

    private sealed class CachedSelectQuerier<TList, TIn, TOut>(TList list, Func<TIn, TOut> selector) : SimpleListQuerier<TList, TIn, TOut>(list) where TList : IReadOnlyList<TIn>
    {
        private TOut[] _cache = null!;
        private byte[]? _isCachedMarks;

        public override TOut this[int index]
        {
            get {
                if (_isCachedMarks is null) {
                    var count = Count;
                    _isCachedMarks = new byte[StackAllocBitArray.GetArrayLength(count)];
                    _cache = new TOut[count];
                }

                Debug.Assert(_cache is not null);
                var bits = new StackAllocBitArray(_isCachedMarks);
                if (bits[index])
                    return _cache[index];

                bits[index] = true;
                return _cache[index] = selector(_list[index]);
            }
        }

        public override int Count => _list.Count;

        protected override EnumerationQuerier<TOut> Clone() => new CachedSelectQuerier<TList, TIn, TOut>(_list, selector);
    }
}
