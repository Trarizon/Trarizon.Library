// Extends Enumerable.Reverse which
// - caches the items in list
// https://source.dot.net/#System.Linq/System/Linq/Reverse.cs,3af306c560f8c669

using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    /// <summary>
    /// Reverse the list
    /// </summary>
    public static IList<T> ReverseList<T>(this IList<T> list)
    {
        if (list.IsFixedSizeAtMost(1))
            return list;

        return new ReverseQuerier<IList<T>, T>(list);
    }

    /// <summary>
    /// Reverse the list
    /// </summary>
    public static IReadOnlyList<T> ReverseROList<T>(this IReadOnlyList<T> list)
    {
        if (list.IsFixedSizeAtMost(1))
            return list;

        return new ReverseQuerier<ListWrapper<T>, T>(list.Wrap());
    }


    private sealed class ReverseQuerier<TList, T>(TList list)
        : SimpleIndexMapListQuerier<TList, T>(list)
        where TList : IList<T>
    {
        public override int Count => _list.Count;

        protected override int ValdiateAndSelectIndex(int index) => _list.Count - 1 - index;

        protected override EnumerationQuerier<T> Clone() => new ReverseQuerier<TList, T>(_list);
    }
}
