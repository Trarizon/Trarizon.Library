namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    public static Optional<T> ToOptional<T, TError>(this in Result<T, TError> result)
    => result.IsSuccess ? Optional.Of(result._value) : Optional.None;

    public static Optional<T> ToOptional<T>(this in Result.SuccessBuilder<T> result)
        => Optional.Of(result.Value);

    public static Optional<TError> ToOptionalError<T, TError>(this in Result<T, TError> result)
        => result.IsFailure ? Optional.Of(result.Error) : Optional.None;

    public static Optional<TError> ToOptionalError<TError>(this in Result.FailureBuilder<TError> result)
        => Optional.Of(result.Error);

    public static Optional<Result<T, TError>> Transpose<T, TError>(this in Result<Optional<T>, TError> result)
    {
        if (result.IsSuccess)
        {
            ref readonly var optional = ref result._value;
            return optional.HasValue ? Optional.Of(Result.Success<T, TError>(optional.Value)) : Optional.None;
        }
        else
        {
            return Optional.Of(Result.Failure<T, TError>(result.Error));
        }
    }
}
