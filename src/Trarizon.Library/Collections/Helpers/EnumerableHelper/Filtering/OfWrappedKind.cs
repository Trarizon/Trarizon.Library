using Trarizon.Library.Collections.Helpers.Queriers;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Collections.Helpers;
partial class EnumerableHelper
{
    public static IEnumerable<T> OfNotNone<T>(this IEnumerable<Optional<T>> source)
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfOptionalNotNoneQuerier<T, Optional<T>>(source);
    }

    public static IEnumerable<T> OfSuccess<T, TError>(this IEnumerable<Result<T, TError>> source) where TError : class
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfEitherLeftQuerier<T, TError, Result<T, TError>>(source);
    }

    public static IEnumerable<TError> OfError<T, TError>(this IEnumerable<Result<T, TError>> source) where TError : class
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfEitherRightQuerier<T, TError, Result<T, TError>>(source);
    }

    public static IEnumerable<TLeft> OfLeft<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> source) where TRight : class
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfEitherLeftQuerier<TLeft, TRight, Either<TLeft, TRight>>(source);
    }

    public static IEnumerable<TRight> OfRight<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> source) where TRight : class
    {
        if (source.IsFixedSizeEmpty())
            return [];

        return new OfEitherRightQuerier<TLeft, TRight, Either<TLeft, TRight>>(source);
    }


    private sealed class OfOptionalNotNoneQuerier<T, TOptional>(IEnumerable<TOptional> source)
        : SimpleWhereSelectEnumerationQuerier<TOptional, T>(source) where TOptional : IOptional<T>
    {
        protected override EnumerationQuerier<T> Clone() => new OfOptionalNotNoneQuerier<T, TOptional>(_source);
        protected override bool TrySetValue(TOptional inVal)
            => inVal.TryGetValue(out _current!);
    }

    private sealed class OfEitherLeftQuerier<T, T2, TEither>(IEnumerable<TEither> source)
        : SimpleWhereSelectEnumerationQuerier<TEither, T>(source) where TEither : IEither<T, T2>
    {
        protected override EnumerationQuerier<T> Clone() => new OfEitherLeftQuerier<T, T2, TEither>(_source);
        protected override bool TrySetValue(TEither inVal)
            => inVal.TryGetLeftValue(out _current!);
    }

    private sealed class OfEitherRightQuerier<T, T2, TEither>(IEnumerable<TEither> source)
        : SimpleWhereSelectEnumerationQuerier<TEither, T2>(source) where TEither : IEither<T, T2>
    {
        protected override EnumerationQuerier<T2> Clone() => new OfEitherRightQuerier<T, T2, TEither>(_source);
        protected override bool TrySetValue(TEither inVal)
            => inVal.TryGetRightValue(out _current!);
    }
}
