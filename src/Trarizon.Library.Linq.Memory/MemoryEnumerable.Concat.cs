
namespace Trarizon.Library.Linq.Memory;
partial class MemoryEnumerable
{
    public static void
        Concat<TSource, TSource2, TMid, TResult, TTranslater, TTranslater2>(
        this MemoryEnumerable<TSource, TMid, TTranslater> source,
        MemoryEnumerable<TSource2, TMid, TTranslater2> second)
        where TTranslater : IMemoryTranslater<TSource, TMid>
        where TTranslater2 : IMemoryTranslater<TSource2, TMid>
    {

    }
}

public struct ConcatTranslater<T>
    : IMemoryTranslater<T, T, T>
{
    public T TryMoveNext(ReadOnlySpan<T> span, ReadOnlySpan<T> span2, out bool hasNext)
    {
        throw new NotImplementedException();
    }
}