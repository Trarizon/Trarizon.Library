using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IReadOnlyList<(T, T2)> CartesianProductList<T, T2>(this IList<T> list, IList<T2> list2)
    {
        if (list.IsFixedSizeEmpty() || list2.IsFixedSizeEmpty())
            return Array.Empty<(T, T2)>();

        return new CartesianProductQuerier<IList<T>, IList<T2>, T, T2>(list, list2);
    }

    public static IReadOnlyList<(T, T2)> CartesianProductROList<T, T2>(this IReadOnlyList<T> list, IReadOnlyList<T2> list2)
    {
        if (list.IsFixedSizeEmpty() || list2.IsFixedSizeEmpty())
            return Array.Empty<(T, T2)>();

        return new CartesianProductQuerier<ListWrapper<T>, ListWrapper<T2>, T, T2>(list.Wrap(), list2.Wrap());
    }


    private sealed class CartesianProductQuerier<TList, TList2, T, T2>(TList list, TList2 list2)
        : SimpleReadOnlyListQuerier<TList, T, (T, T2)>(list)
        where TList : IList<T>
        where TList2 : IList<T2>
    {
        protected override (T, T2) At(int index)
        {
            var (div, rem) = Math.DivRem(index, list2.Count);
            return (_list[div], list2[rem]);
        }

        public override int Count => _list.Count * list2.Count;

        protected override EnumerationQuerier<(T, T2)> Clone() => new CartesianProductQuerier<TList, TList2, T, T2>(_list, list2);
    }
}
