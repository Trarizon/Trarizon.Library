using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    public static Result.SuccessBuilder<TResult> Select<T, TResult>(this Result.SuccessBuilder<T> self, Func<T, TResult> selector)
        => Result.Success(selector(self.Value));

    public static Result<TResult, TError> Select<T, TError, TResult>(this Result<T, TError> self, Func<T, TResult> selector)
        => self.IsSuccess ? Result.Success(selector(self.Value)) : Result.Failure(self.Error);

    public static Result<TResult, TError> Select<TError, TResult>(this Result<Unit, TError> self, Func<TResult> selector)
        => self.IsSuccess ? Result.Success(selector()) : Result.Failure(self.Error);

#if NET9_0_OR_GREATER

    [OverloadResolutionPriority(-1)]
    public static RefResult.SuccessBuilder<TResult> Select<T, TResult>(this Result.SuccessBuilder<T> self, RefFunc<T, TResult> selector)
        where TResult : allows ref struct
        => Result.Success(selector(self.Value));

    public static RefResult.SuccessBuilder<TResult> Select<T, TResult>(this RefResult.SuccessBuilder<T> self, Func<T, TResult> selector)
        where TResult : allows ref struct
        => Result.Success(selector(self.Value));

    [OverloadResolutionPriority(-1)]
    public static RefResult<TResult, TError> Select<T, TError, TResult>(this Result<T, TError> self, RefFunc<T, TResult> selector)
        where TResult : allows ref struct
        => self.IsSuccess ? Result.Success(selector(self.Value)) : RefResult.Failure(self.Error);

    [OverloadResolutionPriority(-1)]
    public static RefResult<TResult, TError> Select<TError, TResult>(this RefResult<Unit, TError> self, RefFunc<TResult> selector)
        where TResult : allows ref struct
        => self.IsSuccess ? Result.Success(selector()) : RefResult.Failure(self.Error);

    public static RefResult<TResult, TError> Select<T, TError, TResult>(this RefResult<T, TError> self, Func<T, TResult> selector)
        where T : allows ref struct
        where TError : allows ref struct
        where TResult : allows ref struct
        => self.IsSuccess ? Result.Success(selector(self.Value)) : RefResult.Failure(self.Error);

    public static RefResult<TResult, TError> Select<TError, TResult>(this RefResult<Unit, TError> self, Func<TResult> selector)
        where TError : allows ref struct
        where TResult : allows ref struct
        => self.IsSuccess ? Result.Success(selector()) : RefResult.Failure(self.Error);

#endif

    public static Result.FailureBuilder<TResultError> SelectError<TError, TResultError>(this Result.FailureBuilder<TError> self, Func<TError, TResultError> selector)
        => Result.Failure(selector(self.Error));

    public static Result<T, TResultError> SelectError<T, TError, TResultError>(this Result<T, TError> self, Func<TError, TResultError> selector)
        => self.IsFailure ? Result.Failure(selector(self.Error)) : Result.Success(self.Value);

#if NET9_0_OR_GREATER

    [OverloadResolutionPriority(-1)]
    public static RefResult.FailureBuilder<TResultError> SelectError<TError, TResultError>(this Result.FailureBuilder<TError> self, RefFunc<TError, TResultError> selector)
        where TResultError : allows ref struct
        => Result.Failure(selector(self.Error));

    public static RefResult.FailureBuilder<TResultError> SelectError<TError, TResultError>(this RefResult.FailureBuilder<TError> self, Func<TError, TResultError> selector)
        where TError : allows ref struct
        where TResultError : allows ref struct
        => Result.Failure(selector(self.Error));

    [OverloadResolutionPriority(-1)]
    public static RefResult<T, TResultError> SelectError<T, TError, TResultError>(this Result<T, TError> self, RefFunc<TError, TResultError> selector)
        where TResultError : allows ref struct
        => self.IsFailure ? Result.Failure(selector(self.Error)) : RefResult.Success(self.Value);

    public static RefResult<T, TResultError> SelectError<T, TError, TResultError>(this RefResult<T, TError> self, Func<TError, TResultError> selector)
        where T : allows ref struct
        where TError : allows ref struct
        where TResultError : allows ref struct
        => self.IsFailure ? Result.Failure(selector(self.Error)) : RefResult.Success(self.Value);

#endif

    public static Result<TResult, TResultError> Select<T, TError, TResult, TResultError>(this Result<T, TError> self, Func<T, TResult> valueSelector, Func<TError, TResultError> errorSelector)
        => self.IsSuccess ? Result.Success(valueSelector(self.Value)) : Result.Failure(errorSelector(self.Error));

#if NET9_0_OR_GREATER

    public static RefResult<TResult, TResultError> Select<T, TError, TResult, TResultError>(this RefResult<T, TError> self, Func<T, TResult> valueSelector, Func<TError, TResultError> errorSelector)
        where T : allows ref struct
        where TError : allows ref struct
        where TResult : allows ref struct
        where TResultError : allows ref struct
        => self.IsSuccess ? Result.Success(valueSelector(self.Value)) : Result.Failure(errorSelector(self.Error));

#endif
}
