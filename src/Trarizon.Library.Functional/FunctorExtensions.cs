namespace Trarizon.Library.Functional;

public static class FunctorExtensions
{
    public static T? ToNullable<T>(this in Optional<T> optional) where T : struct
        => optional.HasValue ? optional.Value : null;

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error)
        => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector)
        => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(errorSelector());

    public static Optional<T> ToOptional<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? Optional.Of(result._value) : Optional.None;

    public static Optional<TError> ToOptionalError<T, TError>(this in Result<T, TError> result)
        => result.IsFailure ? Optional.Of(result.Error) : Optional.None;

    public static Result<Optional<T>, TError> Transpose<T, TError>(this in Optional<Result<T, TError>> optional)
    {
        if (optional.HasValue) {
            ref readonly var result = ref optional.GetValueRefOrDefaultRef();
            return result.IsSuccess ? Optional.Of(result._value) : Result.Failure(result.Error);
        }
        return Result.Success<Optional<T>>(Optional.None);
    }

    public static Optional<Result<T, TError>> Transpose<T, TError>(this in Result<Optional<T>, TError> result)
    {
        if (result.IsSuccess) {
            ref readonly var optional = ref result._value;
            return optional.HasValue ? Optional.Of(Result.Success<T, TError>(optional.Value)) : Optional.None;
        }
        else {
            return Optional.Of(Result.Failure<T, TError>(result.Error));
        }
    }
}
