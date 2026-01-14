namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    public static TResult Match<T, TError, TResult>(this Result<T, TError> self, Func<T, TResult> valueSelector, Func<TError, TResult> errorSelector)
#if NET9_0_OR_GREATER
        where TResult : allows ref struct
#endif
        => self.IsSuccess ? valueSelector(self.Value) : errorSelector(self.Error);

    public static void Match<T, TError>(this Result<T, TError> self, Action<T> valueSelector, Action<TError> errorSelector)
    {
        if (self.IsSuccess)
            valueSelector(self.Value);
        else
            errorSelector(self.Error);
    }

    public static void MatchValue<T, TError>(this Result<T, TError> self, Action<T> selector)
    {
        if (self.IsSuccess) selector(self.Value);
    }

    public static void MatchError<T, TError>(this Result<T, TError> self, Action<TError> selector)
    {
        if (self.IsFailure) selector(self.Error);
    }

#if NET9_0_OR_GREATER

    public static TResult Match<T, TError, TResult>(this RefResult<T, TError> self, Func<T, TResult> valueSelector, Func<TError, TResult> errorSelector)
        where TResult : allows ref struct
        => self.IsSuccess ? valueSelector(self.Value) : errorSelector(self.Error);

    public static void Match<T, TError>(this RefResult<T, TError> self, Action<T> valueSelector, Action<TError> errorSelector)
    {
        if (self.IsSuccess)
            valueSelector(self.Value);
        else
            errorSelector(self.Error);
    }

    public static void MatchValue<T, TError>(this RefResult<T, TError> self, Action<T> selector)
    {
        if (self.IsSuccess) selector(self.Value);
    }

    public static void MatchError<T, TError>(this RefResult<T, TError> self, Action<TError> selector)
    {
        if (self.IsFailure) selector(self.Error);
    }

#endif
}
