// Extends Enumerable.Select which
// does not implement IList<>
// https://source.dot.net/#System.Linq/System/Linq/Select.cs,c669c338f82e311e

using Trarizon.Library.Collections.Extensions.Helpers.Queriers;
using Trarizon.Library.Collections.Extensions.ListQueries.Helpers;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections.Extensions;
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
        private Optional<TOut>[]? _cache;

        public override TOut this[int index]
        {
            get {
                var inVal = _list[index];
                _cache ??= new Optional<TOut>[Count];

                ref var outVal = ref _cache[index];
                if (!outVal.HasValue) {
                    outVal = selector(inVal);
                }

                return outVal.Value;
            }
        }

        public override int Count => _list.Count;

        protected override EnumerationQuerier<TOut> Clone() => new CachedSelectQuerier<TList, TIn, TOut>(_list, selector);
    }
}
