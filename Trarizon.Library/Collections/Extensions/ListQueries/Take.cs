using Trarizon.Library.Collections.Extensions.Helpers.Queriers;
using Trarizon.Library.Collections.Extensions.ListQueries.Helpers;

namespace Trarizon.Library.Collections.Extensions;
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
        return new TakeRangeQuerier<ListWrapper<T>, T>(list.Wrap(), start, length);
    }

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
    public static IReadOnlyList<T> TakeROList<T>(this IReadOnlyList<T> list, Range range)
    {
        if (range.Equals(Range.All))
            return list;

        var (start, length) = range.GetOffsetAndLength(list.Count);
        return new TakeRangeQuerier<IReadOnlyList<T>, T>(list, start, length);
    }


    private sealed class TakeRangeQuerier<TList, T>(TList list, Index start, int length) : SimpleListQuerier<TList, T, T>(list) where TList : IReadOnlyList<T>
    {
        private readonly int _start =
#if NET8_0_OR_GREATER
            int.Clamp(start.GetOffset(list.Count), 0, list.Count);
#else
            start.GetOffset(list.Count) is var value && value < 0 ? 0 : (value > list.Count ? list.Count : value);
#endif
        private readonly int _length = Math.Max(length, 0);

        public override T this[int index]
        {
            get {
                if (index < 0 || index >= Count)
                    ThrowHelper.ThrowArgumentOutOfRange(nameof(index));

                return _list[index + _start];
            }
        }

        public override int Count
        {
            get {
                return Math.Min(_length, _list.Count - _start);
            }
        }

        protected override EnumerationQuerier<T> Clone() => new TakeRangeQuerier<TList, T>(_list, _start, _length);
    }
}
