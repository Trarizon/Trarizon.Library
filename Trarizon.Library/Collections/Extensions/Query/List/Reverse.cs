using Trarizon.Library.Collections.Extensions.Query.Queriers;

namespace Trarizon.Library.Collections.Extensions.Query;
partial class ListQuery
{
    /// <summary>
    /// Reverse the list
    /// </summary>
    public static IList<T> ReverseList<T>(this IList<T> list) => new ReverseQuerier<T>(list);


    private sealed class ReverseQuerier<T>(IList<T> list) : SimpleListQuerier<T, T>(list)
    {
        public override T this[int index] => _list[_list.Count - 1 - index];
        public override int Count => _list.Count;

        protected override EnumerationQuerier<T> Clone() => new ReverseQuerier<T>(_list);
    }
}
