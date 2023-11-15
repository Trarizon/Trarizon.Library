using Trarizon.Library.Collections.Extensions.Query.Queriers;

namespace Trarizon.Library.Collections.Extensions.Query;
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
        return new TakeRangeQuerier<T>(list, start, length);
    }


    private sealed class TakeRangeQuerier<T>(IList<T> list, Index start, int length) : SimpleListQuerier<T, T>(list)
    {
        public override T this[int index]
        {
            get {
                if (index < 0 || index >= length)
                    ThrowHelper.ThrowArgumentOutOfRange(nameof(index));

                return _list[index + start.GetOffset(_list.Count)];
            }
        }

        public override int Count
        {
            get {
                if (start.IsFromEnd && start.Value < length)
                    return 0;
                else if (start.Value + length > _list.Count)
                    return 0;
                else
                    return length;
            }
        }

        protected override EnumerationQuerier<T> Clone() => new TakeRangeQuerier<T>(_list, start, length);
    }
}
