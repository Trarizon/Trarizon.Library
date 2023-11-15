using Trarizon.Library.Collections.Extensions.Query.Queriers;

namespace Trarizon.Library.Collections.Extensions.Query;
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
            return new RotateQuerier<T>(list, splitPosition);
    }


    private sealed class RotateQuerier<T>(IList<T> list, int splitPosition) : SimpleListQuerier<T, T>(list)
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

        protected override EnumerationQuerier<T> Clone() => new RotateQuerier<T>(_list, _splitPosition);
    }

}
