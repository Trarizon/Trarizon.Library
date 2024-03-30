using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    /// <summary>
    /// Swap two parts of the list
    /// </summary>
    public static IList<T> RotateList<T>(this IList<T> list, int splitPosition)
    {
        if (splitPosition == 0 || splitPosition >= list.Count)
            return list;
        else
            return new RotateQuerier<IList<T>, T>(list, splitPosition);
    }

    /// <summary>
    /// Swap two parts of the list
    /// </summary>
    public static IReadOnlyList<T> RotateROList<T>(this IReadOnlyList<T> list, int splitPosition)
    {
        if (splitPosition == 0 || splitPosition >= list.Count)
            return list;
        else
            return new RotateQuerier<ListWrapper<T>, T>(list.Wrap(), splitPosition);
    }


    private sealed class RotateQuerier<TList, T>(TList list, int splitPosition)
        : SimpleIndexMapListQuerier<TList, T>(list)
        where TList : IList<T>
    {
        private readonly int _splitPosition = splitPosition;

        public override int Count => _list.Count;

        protected override int ValdiateAndSelectIndex(int index)
        {
            var count = Count;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);

            var newIndex = _splitPosition + index;
            if (newIndex > count)
                return newIndex - count;
            else
                return newIndex;
        }

        protected override EnumerationQuerier<T> Clone() => new RotateQuerier<TList, T>(_list, _splitPosition);
    }

}
