using System.Collections;

namespace Trarizon.Library.Collections.Extensions.Helper;
internal readonly struct ListWrapper<T> : IReadOnlyList<T>
{
    public readonly IList<T> List;

    public T this[int index] => List[index];

    public int Count => List.Count;

    public IEnumerator<T> GetEnumerator() => List.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)List).GetEnumerator();
}
