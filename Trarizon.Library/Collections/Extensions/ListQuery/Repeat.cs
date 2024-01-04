using Trarizon.Library.Collections.Extensions.Helper.Queriers;
using Trarizon.Library.Collections.Extensions.Helper;

namespace Trarizon.Library.Collections.Extensions;
partial class ListQuery
{
    /// <summary>
    /// Repeat the list for <paramref name="count"/> times
    /// </summary>
    public static IList<T> RepeatList<T>(this IList<T> list, int count)
    {
        if (count == 0)
            return Array.Empty<T>();
        else
            return new RepeatQuerier<ListWrapper<T>, T>(list.Wrap(), count);
    }

    /// <summary>
    /// Repeat the list for <paramref name="count"/> times
    /// </summary>
    public static IReadOnlyList<T> RepeatROList<T>(this IReadOnlyList<T> list, int count)
    {
        if (count == 0)
            return Array.Empty<T>();
        else
            return new RepeatQuerier<IReadOnlyList<T>, T>(list, count);
    }


    private sealed class RepeatQuerier<TList, T>(TList list, int count) : SimpleListQuerier<TList, T, T>(list) where TList : IReadOnlyList<T>
    {
        public override T this[int index] => _list[index % _list.Count];

        public override int Count => _list.Count * count;

        protected override EnumerationQuerier<T> Clone() => new RepeatQuerier<TList, T>(_list, count);
    }
}
