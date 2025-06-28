namespace Trarizon.Library.Linq.Memory;
public static partial class MemoryEnumerable
{
    public static MemoryEnumerable<T> AsMemoryEnumerable<T>(this ReadOnlySpan<T> span)
        => new(span);
}

public readonly ref struct MemoryEnumerable<T>(ReadOnlySpan<T> span)
{
    private readonly ReadOnlySpan<T> _span = span;
}

public readonly ref struct MemoryEnumerable<TSource, T, TTranslater>(
    ReadOnlySpan<TSource> source,
    TTranslater translater)
    where TTranslater : IMemoryTranslater<TSource, T>
{
    internal readonly ReadOnlySpan<TSource> _src = source;
    internal readonly TTranslater _translater = translater;

    public ref struct Enumerator
    {
        private readonly ReadOnlySpan<TSource> _source;
        private readonly TTranslater _translater;
        private int _index;
        private T _current;

        public readonly T Current => _current;

        public bool MoveNext()
        {
        Start:
            int index = _index + 1;
            if (index < _source.Length) {
                _index = index;
                if (_translater.TryTranslate(_source[_index], out var res)) {
                    _current = res;
                    return true;
                }
                else {
                    goto Start;
                }
            }

            return false;
        }
    }
}