namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Result<T, TError> ToResult<T, TError>(this Optional<T> optional, TError error)
      => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(error);

    public static Result<T, TError> ToResult<T, TError>(this Optional<T> optional, Func<TError> errorSelector)
        => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(errorSelector());

    public static Result<Optional<T>, TError> Transpose<T, TError>(this Optional<Result<T, TError>> optional)
    {
        if (optional.HasValue) {
            ref readonly var result = ref optional.GetValueRefOrDefaultRef();
            return result.IsSuccess ? Optional.Of(result._value) : Result.Failure(result.Error);
        }
        return Result.Success(Optional<T>.None);
    }

#if REF_MONAD

    public static RefResult<T, TError> ToResult<T, TError>(this RefOptional<T> optional, TError error) where T : allows ref struct
        => optional.HasValue ? RefResult.Success(optional.Value) : RefResult.Failure(error);

    public static RefResult<T, TError> ToResult<T, TError>(this RefOptional<T> optional, Func<TError> errorSelector) where T : allows ref struct
        => optional.HasValue ? RefResult.Success(optional.Value) : RefResult.Failure(errorSelector());

    public static RefResult<RefOptional<T>, TError> Transpose<T, TError>(this RefOptional<RefResult<T, TError>> optional) where T : allows ref struct
    {
        if (optional.HasValue) {
            ref readonly var result = ref optional.GetValueRefOrDefaultRef();
            return result.IsSuccess ? RefResult.Success(RefOptional.Of(result._value)) : RefResult.Failure(result.Error);
        }
        return RefResult.Success(RefOptional<T>.None);
    }

#endif
}
