using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    /// <summary>
    /// Iterate from specific <paramref name="start"/> to end
    /// </summary>
    /// true if <paramref name="start"/> is binded to <paramref name="list"/>.Count,
    /// similar to IsFromEnd of System.Index
    /// </param>
    public static IList<T> TakeList<T>(this IList<T> list, Index start)
        => list.TakeList(start..);

    /// <summary>
    /// Iterate from specific <paramref name="start"/> to end
    /// </summary>
    /// true if <paramref name="start"/> is binded to <paramref name="list"/>.Count,
    /// similar to IsFromEnd of System.Index
    /// </param>
    public static IReadOnlyList<T> TakeROList<T>(this IReadOnlyList<T> list, Index start)
        => list.TakeROList(start..);


    /// <summary>
    /// Iterate items in specific <paramref name="range"/>.
    /// </summary>
    public static IList<T> TakeList<T>(this IList<T> list, Range range)
    {
        if (range.Equals(Range.All))
            return list;

        if (list.IsFixedSize() && range.GetOffsetAndLength(list.Count).Length == 0)
            return Array.Empty<T>();

        return new TakeRangeQuerier<IList<T>, T>(list, range);
    }

    /// <summary>
    /// Iterate items in specific <paramref name="range"/>.
    /// </summary>
    public static IReadOnlyList<T> TakeROList<T>(this IReadOnlyList<T> list, Range range)
    {
        if (range.Equals(Range.All))
            return list;

        if (list.IsFixedSize() && range.GetOffsetAndLength(list.Count).Length == 0)
            return Array.Empty<T>();

        return new TakeRangeQuerier<ListWrapper<T>, T>(list.Wrap(), range);
    }


    private sealed class TakeRangeQuerier<TList, T>(TList list, Range range)
        : SimpleIndexMapListQuerier<TList, T>(list)
        where TList : IList<T>
    {
        public override int Count
        {
            get {
                var lstCount = _list.Count;
                var (start, length) = range.GetOffsetAndLength(lstCount);
                return int.Clamp(length, 0, lstCount - start);
            }
        }

        protected override int ValdiateAndSelectIndex(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            var lstCount = _list.Count;
            var (start, length) = range.GetOffsetAndLength(lstCount);
            var count = int.Clamp(length, 0, lstCount - start);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);
            return index + start;
        }

        protected override EnumerationQuerier<T> Clone() => new TakeRangeQuerier<TList, T>(_list, range);
    }
}
