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

    public static IEnumerable<TResult> WhereSelect<T, TResult>(this IEnumerable<T> source, Func<T, TResult?> whereSelector) where TResult : struct
    {
        if (source.IsFixedSizeEmpty())
            return [];
        return new WhereSelectValueTypeQuerier<T, TResult>(source, whereSelector);
    }

    public static IEnumerable<TResult> WhereSelect<T, TResult>(this IEnumerable<T> source, Func<T, TResult?> whereSelector) where TResult : class
    {
        if (source.IsFixedSizeEmpty())
            return [];
        return new WhereSelectReferenceTypeQuerier<T, TResult>(source, whereSelector);
    }


    private sealed class WhereSelectQuerier<T, TResult>(IEnumerable<T> source, Func<T, Optional<TResult>> whereSelector)
        : SimpleWhereSelectEnumerationQuerier<T, TResult>(source)
    {
        protected override EnumerationQuerier<TResult> Clone() => new WhereSelectQuerier<T, TResult>(_source, whereSelector);
        protected override bool TrySetValue(T inVal)
            => whereSelector(inVal).TryGetValue(out _current!);
    }

    private sealed class WhereSelectValueTypeQuerier<T, TResult>(IEnumerable<T> source, Func<T, TResult?> whereSelector)
        : SimpleWhereSelectEnumerationQuerier<T, TResult>(source)
        where TResult : struct
    {
        protected override EnumerationQuerier<TResult> Clone() => new WhereSelectValueTypeQuerier<T, TResult>(_source, whereSelector);
        protected override bool TrySetValue(T inVal)
        {
            var t = whereSelector(inVal);
            if (t is null)
                return false;

            _current = t.GetValueOrDefault();
            return true;
        }
    }

    private sealed class WhereSelectReferenceTypeQuerier<T, TResult>(IEnumerable<T> source, Func<T, TResult?> whereSelector)
        : SimpleWhereSelectEnumerationQuerier<T, TResult>(source)
        where TResult : class
    {
        protected override EnumerationQuerier<TResult> Clone() => new WhereSelectReferenceTypeQuerier<T, TResult>(_source, whereSelector);
        protected override bool TrySetValue(T inVal)
        {
            var t = whereSelector(inVal);
            if (t is null)
                return false;

            _current = t;
            return true;
        }
    }
}
