using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers.Utilities.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableQuery
{
    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        if (source.IsCheapEmpty())
            return Enumerable.Empty<T>();
        return new OfReferenceTypeNotNullQuerier<T>(source);
    }

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : struct
    {
        if (source.IsCheapEmpty())
            return Enumerable.Empty<T>();
        return new OfValueTypeNotNullQuerier<T>(source);
    }


    private sealed class OfReferenceTypeNotNullQuerier<T>(IEnumerable<T?> source) : SimpleWhereSelectEnumerationQuerier<T?, T>(source) where T : class
    {
        protected override EnumerationQuerier<T> Clone() => new OfReferenceTypeNotNullQuerier<T>(_source);

        protected override bool TrySelect(T? inVal, [MaybeNullWhen(false)] out T outVal)
        {
            outVal = inVal;
            return inVal is not null;
        }
    }

    private sealed class OfValueTypeNotNullQuerier<T>(IEnumerable<T?> source) : SimpleWhereSelectEnumerationQuerier<T?, T>(source) where T : struct
    {
        protected override EnumerationQuerier<T> Clone() => new OfValueTypeNotNullQuerier<T>(_source);
        protected override bool TrySelect(T? inVal, [MaybeNullWhen(false)] out T outVal)
        {
            outVal = inVal.GetValueOrDefault();
            return inVal is not null;
        }
    }
}
