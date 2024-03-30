using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    /// <summary>
    /// Iterate from specific <paramref name="start"/> to end
    /// </summary>
    /// <param name="backward">Reversing iteration</param>
    /// <param name="indexFromEnd">
    /// true if <paramref name="start"/> is binded to <paramref name="list"/>.Count,
    /// similar to IsFromEnd of System.Index
    /// </param>
    public static IList<T> TakeList<T>(this IList<T> list, Index start)
        => list.TakeList(start..);

    /// <summary>
    /// Iterate from specific <paramref name="start"/> to end
    /// </summary>
    /// <param name="backward">Reversing iteration</param>
    /// <param name="indexFromEnd">
    /// true if <paramref name="start"/> is binded to <paramref name="list"/>.Count,
    /// similar to IsFromEnd of System.Index
    /// </param>
    public static IReadOnlyList<T> TakeROList<T>(this IReadOnlyList<T> list, Index start)
        => list.TakeROList(start..);


    /// <summary>
    /// Iterate items in specific <paramref name="range"/>.
    /// Iteration contains <paramref name="range"/>.Start and doesn't contains <paramref name="range"/>.End,
    /// irrelevant to <paramref name="reverse"/>
    /// </summary>
    /// <param name="reverse">Reversing iteration</param>
    public static IList<T> TakeList<T>(this IList<T> list, Range range)
    {
        if (range.Equals(Range.All))
            return list;

        var (start, length) = range.GetOffsetAndLength(list.Count);
        return new TakeRangeQuerier<IList<T>, T>(list, start, length);
    }

    /// <summary>
    /// Iterate items in specific <paramref name="range"/>.
    /// Iteration contains <paramref name="range"/>.Start and doesn't contains <paramref name="range"/>.End,
    /// irrelevant to <paramref name="reverse"/>
    /// </summary>
    /// <param name="reverse">Reversing iteration</param>
    public static IReadOnlyList<T> TakeROList<T>(this IReadOnlyList<T> list, Range range)
    {
        if (range.Equals(Range.All))
            return list;

        var (start, length) = range.GetOffsetAndLength(list.Count);
        return new TakeRangeQuerier<ListWrapper<T>, T>(list.Wrap(), start, length);
    }


    private sealed class TakeRangeQuerier<TList, T>(TList list, int start, int length)
        : SimpleIndexMapListQuerier<TList, T>(list)
        where TList : IList<T>
    {
        private readonly int _length = Math.Max(length, 0);

        public override int Count
        {
            get {
                return Math.Min(_length, _list.Count - start);
            }
        }

        protected override int ValdiateAndSelectIndex(int index)
        {
            ValidateNegative(index);
            ValidateOutOfRange(index);
            return index + start;
        }

        protected override EnumerationQuerier<T> Clone() => new TakeRangeQuerier<TList, T>(_list, start, _length);
    }
}
