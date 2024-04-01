using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IReadOnlyList<(T, T)> AdjacentList<T>(this IList<T> list)
    {
        if (list.IsFixedSizeAtMost(1))
            return Array.Empty<(T, T)>();

        return new AdjacentQuerier<IList<T>, T>(list);
    }

    public static IReadOnlyList<(T, T)> AdjacentROList<T>(this IReadOnlyList<T> list)
    {
        if (list.IsFixedSizeAtMost(1))
            return Array.Empty<(T, T)>();

        return new AdjacentQuerier<ListWrapper<T>, T>(list.Wrap());
    }


    private sealed class AdjacentQuerier<TList, T>(TList list)
        : SimpleReadOnlyListQuerier<TList, T, (T, T)>(list)
        where TList : IList<T>
    {
        protected override (T, T) At(int index) => (_list[index], _list[index + 1]);

        public override int Count => int.Max(_list.Count - 1, 0);

        protected override EnumerationQuerier<(T, T)> Clone() => new AdjacentQuerier<TList, T>(_list);
    }
}
