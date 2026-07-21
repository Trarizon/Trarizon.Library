namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error)
      => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector)
        => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(errorSelector());

    public static Result<Optional<T>, TError> Transpose<T, TError>(this in Optional<Result<T, TError>> optional)
    {
        if (optional.HasValue) {
            ref readonly var result = ref optional.GetValueRefOrDefaultRef();
            return result.IsSuccess ? Optional.Of(result._value) : Result.Failure(result.Error);
        }
        return Result.Success<Optional<T>>(Optional.None);
    }
}
