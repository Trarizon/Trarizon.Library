using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    private abstract class SimpleEnumerationQuerier<TIn, TOut>(IEnumerable<TIn> source) 
        : EnumerationQuerier<TOut>
    {
        protected readonly IEnumerable<TIn> _source = source;
        protected IEnumerator<TIn> _enumerator = default!;
        protected TOut _current = default!;

        public sealed override TOut Current => _current;

        [MemberNotNull(nameof(_enumerator))]
        protected void InitializeEnumerator() => _enumerator = _source.GetEnumerator();

        protected override void DisposeInternal() => _enumerator?.Dispose();
    }
}