namespace Trarizon.Library.Linq.Memory;
internal interface IMemoryEnumerationHelper<TSource, T, TResult>
{
    TResult MoveNext(Enumer<TSource, T> span, out bool hasNext);
}

internal interface IMemoryEnumerationHelper<TSource1, T1, TSource2, T2, TResult>
{
    TResult MoveNext(Enumer<TSource1, T1> span1, Enumer<TSource2, T2> span2, out bool hasNext);
}

internal interface IMemoryTranslater<T, TResult>
{
    TResult Translate(T value);
}

ref struct Enumer<TSource, TResult>
{
    private ReadOnlySpan<TSource> _source;

    public TResult MoveNext(out bool hasNext)
    {
        dynamic src = default!, dst = default!;
        src
            .Concat(dst) // Enumer<TS, TR, Enumer<TS,TR>>
            .Concat(src);

    }
}

ref struct Enumer<TSource, TResult>

struct Select<T, TResult>(Func<T, TResult> selector)
    : IMemoryTranslater<T, TResult>
{
    public readonly TResult Translate(T value) => selector(value);
}

struct Concat<TSource, T>
    : IMemoryEnumerationHelper<TSource, T, TSource, T, T>
{
    public readonly T MoveNext(Enumer<TSource, T> span1, Enumer<TSource, T> span2, out bool hasNext)
    {
        var rtn = span1.MoveNext(out hasNext);
        if (hasNext)
            return rtn;
        rtn = span2.MoveNext(out hasNext);
        return rtn;
    }
}

struct ChainEH<TSource, TMid, TResult, TPrevEnum, TNextEnum>(
    TPrevEnum prev, TNextEnum next)
    : IMemoryEnumerationHelper<TSource, TResult>
    where TPrevEnum : IMemoryEnumerationHelper<TSource, TMid>, IMemoryTranslater<TSource, TMid>
    where TNextEnum : IMemoryEnumerationHelper<TMid, TResult>, IMemoryTranslater<TMid, TResult>
{
    public TResult MoveNext(ReadOnlySpanEnumerator<TSource> span, out bool hasNext)
    {
        throw new NotImplementedException();
    }
}