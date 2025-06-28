namespace Trarizon.Library.Linq.Memory;
internal ref struct ReadOnlySpanEnumerator<T>
{
    internal ReadOnlySpan<T> _items;
    internal int _index;

    public ref readonly T MoveNext(out bool hasNext)
    {
    }
}
