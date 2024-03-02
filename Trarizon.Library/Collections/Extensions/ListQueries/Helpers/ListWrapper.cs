using System.Collections;

namespace Trarizon.Library.Collections.Extensions.ListQueries.Helpers;
internal readonly struct ListWrapper<T> : IReadOnlyList<T>
{
#nullable disable
    public readonly IList<T> List;
#nullable restore

    public T this[int index] => List[index];

    public int Count => List.Count;

    public IEnumerator<T> GetEnumerator() => List.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)List).GetEnumerator();
}
