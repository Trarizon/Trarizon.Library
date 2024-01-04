using Trarizon.Library.Collections.Extensions.Helpers.Queriers;
using Trarizon.Library.Collections.Extensions.ListQueries.Helpers;

namespace Trarizon.Library.Collections.Extensions;
partial class ListQuery
{
    /// <summary>
    /// Reverse the list
    /// </summary>
    public static IList<T> ReverseList<T>(this IList<T> list) => new ReverseQuerier<ListWrapper<T>, T>(list.Wrap());

    /// <summary>
    /// Reverse the list
    /// </summary>
    public static IReadOnlyList<T> ReverseROList<T>(this IReadOnlyList<T> list) => new ReverseQuerier<IReadOnlyList<T>, T>(list);


    private sealed class ReverseQuerier<TList, T>(TList list) : SimpleListQuerier<TList, T, T>(list) where TList : IReadOnlyList<T>
    {
        public override T this[int index] => _list[_list.Count - 1 - index];
        public override int Count => _list.Count;

        protected override EnumerationQuerier<T> Clone() => new ReverseQuerier<TList, T>(_list);
    }
}
