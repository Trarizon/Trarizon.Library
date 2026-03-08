namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    public static Result.FailureBuilder<T> Swap<T>(this Result.SuccessBuilder<T> self)
        => Result.Failure(self.Value);

    public static Result.SuccessBuilder<T> Swap<T>(this Result.FailureBuilder<T> self) 
        => Result.Success(self.Error);

    public static Result<TError, T> Swap<T, TError>(this Result<T, TError> self)
        => Result.Create(self.IsFailure, self.GetErrorOrDefault()!, self.GetValueOrDefault()!);

#if NET9_0_OR_GREATER

    public static RefResult.FailureBuilder<T> Swap<T>(this RefResult.SuccessBuilder<T> self)
        => RefResult.Failure(self.Value);

    public static RefResult.SuccessBuilder<T> Swap<T>(this RefResult.FailureBuilder<T> self) 
        => RefResult.Success(self.Error);

    public static RefResult<TError, T> Swap<T, TError>(this RefResult<T, TError> self)
        => Result.Create(self.IsFailure, self.GetErrorOrDefault()!, self.GetValueOrDefault()!);

#endif
}
