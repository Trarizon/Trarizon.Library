namespace Trarizon.Library.Collections.Extensions.Query.Queriers;
internal abstract class SimpleEnumerationQuerier<TIn, TOut> : EnumerationQuerier<TOut>
{
    protected readonly IEnumerable<TIn> _source;
    protected IEnumerator<TIn>? _enumerator;
    protected TOut _current = default!;

    protected SimpleEnumerationQuerier(IEnumerable<TIn> source) => _source = source;

    public override TOut Current => _current;

    protected override void DisposeInternal() => _enumerator?.Dispose();
}
