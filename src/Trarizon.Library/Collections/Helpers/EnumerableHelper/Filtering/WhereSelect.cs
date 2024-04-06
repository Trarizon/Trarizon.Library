using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers.Queriers;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    /// <summary>
    /// Combine Where and Select, so you can use intermediate variables
    /// </summary>
    public static IEnumerable<TResult> WhereSelect<T, TResult>(this IEnumerable<T> source, Func<T, Optional<TResult>> whereSelector)
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new WhereSelectQuerier<T, TResult>(source, whereSelector);
    }

    private sealed class WhereSelectQuerier<T, TResult>(IEnumerable<T> source, Func<T, Optional<TResult>> whereSelector)
        : SimpleWhereSelectEnumerationQuerier<T, TResult>(source)
    {
        protected override EnumerationQuerier<TResult> Clone() => new WhereSelectQuerier<T, TResult>(_source, whereSelector);

        protected override bool TrySelect(T inVal, [MaybeNullWhen(false)] out TResult outVal)
            => whereSelector(inVal).TryGetValue(out outVal);
    }
}
