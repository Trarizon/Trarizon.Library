using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IList<(T, T)> AdjacentList<T>(this IList<T> list)
        => new AdjacentQuerier<ListWrapper<T>, T>(list.Wrap());

    public static IReadOnlyList<(T, T)> AdjacentList<T>(this IReadOnlyList<T> list)
        => new AdjacentQuerier<IReadOnlyList<T>, T>(list);


    private sealed class AdjacentQuerier<TList, T>(TList list) : SimpleListQuerier<TList, T, (T, T)>(list) where TList : IReadOnlyList<T>
    {
        public override (T, T) this[int index] => (_list[index], _list[index + 1]);

        public override int Count => _list.Count - 1;

        protected override EnumerationQuerier<(T, T)> Clone() => new AdjacentQuerier<TList, T>(_list);
    }
}
