using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    public static IList<T> TakeEveryList<T>(this IList<T> list, int interval)
    {
        if (interval <= 1)
            return list;

        if (list.IsFixedSizeAtMost(1))
            return list;

        return new TakeEveryQuerier<IList<T>, T>(list, interval);
    }

    public static IReadOnlyList<T> TakeEveryROList<T>(this IReadOnlyList<T> list, int interval)
    {
        if (interval <= 1)
            return list;

        if (list.IsFixedSizeAtMost(1))
            return list;

        return new TakeEveryQuerier<ListWrapper<T>, T>(list.Wrap(), interval);
    }


    private sealed class TakeEveryQuerier<TList, T>(TList list, int interval)
        : SimpleIndexMapListQuerier<TList, T>(list)
        where TList : IList<T>
    {
        public override int Count => _list.Count / interval;

        protected override EnumerationQuerier<T> Clone() => new TakeEveryQuerier<TList, T>(_list, interval);

        protected override int ValdiateAndSelectIndex(int index) => index * interval;
    }
}
