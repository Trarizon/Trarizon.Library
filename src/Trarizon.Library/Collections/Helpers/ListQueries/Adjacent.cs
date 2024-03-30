using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IReadOnlyList<(T, T)> AdjacentList<T>(this IList<T> list)
        => new AdjacentQuerier<IList<T>, T>(list);

    public static IReadOnlyList<(T, T)> AdjacentROList<T>(this IReadOnlyList<T> list)
        => new AdjacentQuerier<ListWrapper<T>, T>(list.Wrap());


    private sealed class AdjacentQuerier<TList, T>(TList list) 
        : SimpleListQuerier<TList, T, (T, T)>(list) 
        where TList : IList<T>
    {
        public override (T, T) this[int index] => (_list[index], _list[index + 1]);

        public override int Count => _list.Count - 1;

        protected override EnumerationQuerier<(T, T)> Clone() => new AdjacentQuerier<TList, T>(_list);
    }
}
