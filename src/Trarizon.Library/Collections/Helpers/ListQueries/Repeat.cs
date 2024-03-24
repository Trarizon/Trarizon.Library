using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
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


    /// <summary>
    /// Repeat the list forever, The returned collection has size <see cref="int.MaxValue"/> unless the source collection is empty
    /// </summary>
    public static IList<T> RepeatForeverList<T>(this IList<T> list)
    {
        if (list.Count == 0)
            return Array.Empty<T>();
        else
            return new RepeatForeverQuerier<ListWrapper<T>, T>(list.Wrap());
    }

    /// <summary>
    /// Repeat the list forever, The returned collection has size <see cref="int.MaxValue"/> unless the source collection is empty
    /// </summary>
    public static IReadOnlyList<T> RepeatForeverROList<T>(this IReadOnlyList<T> list)
    {
        if (list.Count == 0)
            return Array.Empty<T>();
        else
            return new RepeatForeverQuerier<IReadOnlyList<T>, T>(list);
    }


    private sealed class RepeatQuerier<TList, T>(TList list, int count) : SimpleListQuerier<TList, T, T>(list) where TList : IReadOnlyList<T>
    {
        public override T this[int index] => _list[index % _list.Count];

        public override int Count => _list.Count * count;

        protected override EnumerationQuerier<T> Clone() => new RepeatQuerier<TList, T>(_list, count);
    }

    private sealed class RepeatForeverQuerier<TList, T>(TList list) : SimpleListQuerier<TList, T, T>(list) where TList : IReadOnlyList<T>
    {
        public override T this[int index] => _list[index % _list.Count];

        public override int Count => int.MaxValue;

        protected override EnumerationQuerier<T> Clone() => new RepeatForeverQuerier<TList, T>(_list);
    }
}
