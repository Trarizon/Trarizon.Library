using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IList<(T, T2)> CartesianProductList<T, T2>(this IList<T> list, IList<T2> list2)
    {
        if (list.Count == 0 || list2.Count == 0)
            return Array.Empty<(T, T2)>();
        return new CartesianProductQuerier<ListWrapper<T>, ListWrapper<T2>, T, T2>(list.Wrap(), list2.Wrap());
    }

    public static IReadOnlyList<(T, T2)> GetCartesianProductROList<T, T2>(this IReadOnlyList<T> list, IReadOnlyList<T2> list2)
    {
        if (list.Count == 0 || list2.Count == 0)
            return Array.Empty<(T, T2)>();
        return new CartesianProductQuerier<IReadOnlyList<T>, IReadOnlyList<T2>, T, T2>(list, list2);
    }


    private sealed class CartesianProductQuerier<TList, TList2, T, T2>(TList list, TList2 list2)
        : SimpleListQuerier<TList, T, (T, T2)>(list)
        where TList : IReadOnlyList<T>
        where TList2 : IReadOnlyList<T2>
    {
        public override (T, T2) this[int index]
        {
            get {
                var (div, rem) = Math.DivRem(index, list2.Count);
                return (_list[div], list2[rem]);
            }
        }

        public override int Count => _list.Count * list2.Count;

        protected override EnumerationQuerier<(T, T2)> Clone() => new CartesianProductQuerier<TList, TList2, T, T2>(_list, list2);
    }
}
