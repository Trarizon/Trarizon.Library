using Trarizon.Library.Collections.Helpers.Queriers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfReferenceTypeNotNullQuerier<T>(source);
    }

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : struct
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfValueTypeNotNullQuerier<T>(source);
    }


    private sealed class OfReferenceTypeNotNullQuerier<T>(IEnumerable<T?> source)
        : SimpleWhereSelectEnumerationQuerier<T?, T>(source) where T : class
    {
        protected override EnumerationQuerier<T> Clone() => new OfReferenceTypeNotNullQuerier<T>(_source);

        protected override bool TrySetValue(T? inVal)
        {
            if (inVal is null)
                return false;

            _current = inVal;
            return true;
        }
    }

    private sealed class OfValueTypeNotNullQuerier<T>(IEnumerable<T?> source)
        : SimpleWhereSelectEnumerationQuerier<T?, T>(source) where T : struct
    {
        protected override EnumerationQuerier<T> Clone() => new OfValueTypeNotNullQuerier<T>(_source);
        protected override bool TrySetValue(T? inVal)
        {
            if (inVal is null)
                return false;

            _current = inVal.GetValueOrDefault();
            return true;
        }
    }
}
