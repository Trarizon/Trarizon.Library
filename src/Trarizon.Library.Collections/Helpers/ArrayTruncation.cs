using System.Collections;

namespace Trarizon.Library.Collections.Helpers;
internal readonly struct ArrayTruncation<T>(T[] array, int length) : IReadOnlyList<T>
{
    public T this[int index] => array[index];

    public int Count => length;

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in array) {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
