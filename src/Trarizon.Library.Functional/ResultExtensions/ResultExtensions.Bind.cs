using System.ComponentModel;

namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    public static Result<TResult, TError> Bind<T, TError, TResult>(this Result.SuccessBuilder<T> self, Func<T, Result<TResult, TError>> selector)
        => selector(self.Value);

    public static Result<TResult, TError> Bind<T, TError, TResult>(this Result<T, TError> self, Func<T, Result<TResult, TError>> selector)
        => self.IsSuccess ? selector(self.Value) : Result.Failure(self.Error);

#if NET9_0_OR_GREATER

    public static RefResult<TResult, TError> Bind<T, TError, TResult>(this Result.SuccessBuilder<T> self, Func<T, RefResult<TResult, TError>> selector)
        where TError : allows ref struct
        where TResult : allows ref struct
        => selector(self.Value);

    public static RefResult<TResult, TError> Bind<T, TError, TResult>(this RefResult.SuccessBuilder<T> self, Func<T, RefResult<TResult, TError>> selector)
        where TError : allows ref struct
        where TResult : allows ref struct
        => selector(self.Value);

    public static RefResult<TResult, TError> Bind<T, TError, TResult>(this Result<T, TError> self, Func<T, RefResult<TResult, TError>> selector)
        where TResult : allows ref struct
        => self.IsSuccess ? selector(self.Value) : new(self.Error);

    public static RefResult<TResult, TError> Bind<T, TError, TResult>(this RefResult<T, TError> self, Func<T, RefResult<TResult, TError>> selector)
        where TResult : allows ref struct
        => self.IsSuccess ? selector(self.Value) : new(self.Error);

#endif

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<TResult, TError> SelectMany<T, TError, TResult>(this Result<T, TError> self, Func<T, Result<TResult, TError>> selector)
        => self.Bind(selector);

#if NET9_0_OR_GREATER

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefResult<TResult, TError> SelectMany<T, TError, TResult>(this Result<T, TError> self, Func<T, RefResult<TResult, TError>> selector)
        where TResult : allows ref struct
        => self.Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefResult<TResult, TError> SelectMany<T, TError, TResult>(this RefResult<T, TError> self, Func<T, RefResult<TResult, TError>> selector)
        where TResult : allows ref struct
        => self.Bind(selector);


#endif

    public static Result<TResult, TError> Bind<T, TError, TMid, TResult>(this Result<T, TError> self, Func<T, Result<TMid, TError>> selector, Func<T, TMid, TResult> resultSelector)
        => self.IsSuccess && selector(self.Value) is { IsSuccess: true } mid ? Result.Success(resultSelector(self.Value, mid.Value)) : Result.Failure(self.Error);

#if NET9_0_OR_GREATER

    public static Result<TResult, TError> Bind<T, TError, TMid, TResult>(this RefResult<T, TError> self, Func<T, RefResult<TMid, TError>> selector, Func<T, TMid, TResult> resultSelector)
        => self.IsSuccess && selector(self.Value) is { IsSuccess: true } mid ? Result.Success(resultSelector(self.Value, mid.Value)) : Result.Failure(self.Error);

#endif

    public static Result<TResult, TResultError> BindError<TError, TResult, TResultError>(this Result.FailureBuilder<TError> self, Func<TError, Result<TResult, TResultError>> selector)
        => selector(self.Error);

    public static Result<T, TResultError> BindError<T, TError, TResultError>(this Result<T, TError> self, Func<TError, Result<T, TResultError>> selector)
        => self.IsFailure ? selector(self.Error) : Result.Success(self.Value);

#if NET9_0_OR_GREATER

    public static RefResult<TResult, TResultError> BindError<TError, TResult, TResultError>(this Result.FailureBuilder<TError> self, Func<TError, RefResult<TResult, TResultError>> selector)
        where TResult : allows ref struct
        where TResultError : allows ref struct
        => selector(self.Error);

    public static RefResult<TResult, TResultError> BindError<TError, TResult, TResultError>(this RefResult.FailureBuilder<TError> self, Func<TError, RefResult<TResult, TResultError>> selector)
        where TResult : allows ref struct
        where TResultError : allows ref struct
        => selector(self.Error);

    public static RefResult<T, TResultError> BindError<T, TError, TResultError>(this Result<T, TError> self, Func<TError, RefResult<T, TResultError>> selector)
        where TResultError : allows ref struct
        => self.IsFailure ? selector(self.Error) : RefResult.Success(self.Value);

    public static RefResult<T, TResultError> BindError<T, TError, TResultError>(this RefResult<T, TError> self, Func<TError, RefResult<T, TResultError>> selector)
        where TResultError : allows ref struct
        => self.IsFailure ? selector(self.Error) : RefResult.Success(self.Value);

#endif

    public static Result<T, TResultError> BindError<T, TError, TMidError, TResultError>(this Result<T, TError> self, Func<TError, Result<T, TMidError>> selector, Func<TError, TMidError, TResultError> resultSelector)
        => self.IsFailure && selector(self.Error) is { IsFailure: true } mid ? Result.Failure(resultSelector(self.Error, mid.Error)) : Result.Success(self.Value);
}
