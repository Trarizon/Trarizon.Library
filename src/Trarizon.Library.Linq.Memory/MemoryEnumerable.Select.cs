namespace Trarizon.Library.Linq.Memory;
partial class MemoryEnumerable
{
    public static MemoryEnumerable<TSource, TResult, ChainTranslater<TSource, TMid, TResult, TTranslater, SelectTranslater<TMid, TResult>>> Select<TSource, TMid, TResult, TTranslater>(
        this MemoryEnumerable<TSource, TMid, TTranslater> source, Func<TMid, TResult> selector)
        where TTranslater : IMemoryTranslater<TSource, TMid>
    {
        return new(source._src, new(source._translater, new SelectTranslater<TMid, TResult>(selector)));
    }
}

public readonly struct SelectTranslater<T, TResult>(Func<T, TResult> selector)
    : IMemoryTranslater<T, TResult>
{
    public readonly bool TryTranslate(T current, out TResult result)
    {
        result = selector(current);
        return true;
    }
}