using Trarizon.Library.Collections.Extensions.Helpers.Queriers;
using Trarizon.Library.Collections.Extensions.ListQueries.Helpers;

namespace Trarizon.Library.Collections.Extensions;
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
            return new RotateQuerier<ListWrapper<T>, T>(list.Wrap(), splitPosition);
    }

    /// <summary>
    /// Swap two parts of the list
    /// </summary>
    public static IReadOnlyList<T> RotateROList<T>(this IReadOnlyList<T> list, int splitPosition)
    {
        if (splitPosition == 0 || splitPosition >= list.Count)
            return list;
        else
            return new RotateQuerier<IReadOnlyList<T>, T>(list, splitPosition);
    }


    private sealed class RotateQuerier<TList, T>(TList list, int splitPosition) : SimpleListQuerier<TList, T, T>(list) where TList : IReadOnlyList<T>
    {
        private readonly int _splitPosition = splitPosition;

        public override T this[int index]
        {
            get {
                if (index > Count)
                    ThrowHelper.ThrowArgumentOutOfRange(nameof(index));
                return _list[(_splitPosition + index) % _list.Count];
            }
        }
        public override int Count => _list.Count;

        protected override EnumerationQuerier<T> Clone() => new RotateQuerier<TList, T>(_list, _splitPosition);
    }

}
