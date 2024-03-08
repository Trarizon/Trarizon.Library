namespace Trarizon.Library.Collections.Helpers.Utilities.Queriers;
internal abstract class SimpleEnumerationQuerier<TIn, TOut>(IEnumerable<TIn> source) : EnumerationQuerier<TOut>
{
    protected readonly IEnumerable<TIn> _source = source;
    protected IEnumerator<TIn>? _enumerator;
    protected TOut _current = default!;

    public sealed override TOut Current => _current;

    protected override void DisposeInternal() => _enumerator?.Dispose();
}
