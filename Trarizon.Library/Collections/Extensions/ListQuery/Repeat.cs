using Trarizon.Library.Collections.Extensions.Helper.Queriers;

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
            return new RepeatQuerier<T>(list, count);
    }


    private sealed class RepeatQuerier<T>(IList<T> list, int count) : SimpleListQuerier<T, T>(list)
    {
        public override T this[int index] => _list[index % _list.Count];

        public override int Count => _list.Count * count;

        protected override EnumerationQuerier<T> Clone() => new RepeatQuerier<T>(_list, count);
    }
}
