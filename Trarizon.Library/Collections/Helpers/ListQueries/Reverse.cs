// Extends Enumerable.Reverse which
// - caches the items in list
// https://source.dot.net/#System.Linq/System/Linq/Reverse.cs,3af306c560f8c669

using Trarizon.Library.Collections.Helpers.Utilities.Queriers;
using Trarizon.Library.Collections.Helpers.ListQueries.Helpers;

namespace Trarizon.Library.Collections.Helpers;
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
