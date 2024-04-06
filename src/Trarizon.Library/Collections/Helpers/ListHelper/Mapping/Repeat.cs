using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    /// <summary>
    /// Repeat the list for <paramref name="count"/> times
    /// </summary>
    public static IReadOnlyList<T> RepeatList<T>(this IList<T> list, int count)
    {
        if (count == 0 || list.IsFixedSizeEmpty())
            return Array.Empty<T>();

        return new RepeatQuerier<IList<T>, T>(list, count);
    }

    /// <summary>
    /// Repeat the list for <paramref name="count"/> times
    /// </summary>
    public static IReadOnlyList<T> RepeatROList<T>(this IReadOnlyList<T> list, int count)
    {
        if (count == 0 || list.IsFixedSizeEmpty())
            return Array.Empty<T>();

        return new RepeatQuerier<ListWrapper<T>, T>(list.Wrap(), count);
    }

    private sealed class RepeatQuerier<TList, T>(TList list, int count)
        : SimpleReadOnlyListQuerier<TList, T, T>(list)
        where TList : IList<T>
    {
        protected override T At(int index) => _list[index % _list.Count];

        public override int Count => _list.Count * count;

        protected override EnumerationQuerier<T> Clone() => new RepeatQuerier<TList, T>(_list, count);
    }
}
