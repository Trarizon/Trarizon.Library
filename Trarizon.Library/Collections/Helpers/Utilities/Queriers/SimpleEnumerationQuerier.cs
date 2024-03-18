using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Helpers.Utilities.Queriers;
internal abstract class SimpleEnumerationQuerier<TIn, TOut>(IEnumerable<TIn> source) : EnumerationQuerier<TOut>
{
    protected readonly IEnumerable<TIn> _source = source;
    protected IEnumerator<TIn> _enumerator = default!;
    protected TOut _current = default!;

    public sealed override TOut Current => _current;

    [MemberNotNull(nameof(_enumerator))]
    protected void InitializeEnumerator() => _enumerator = _source.GetEnumerator();

    protected override void DisposeInternal() => _enumerator?.Dispose();
}
