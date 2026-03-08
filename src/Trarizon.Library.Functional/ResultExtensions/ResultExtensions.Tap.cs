namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    public static Result<T, TError> Tap<T, TError>(this Result<T, TError> self, Action<T> action)
    {
        if (self.IsSuccess)
            action(self.Value);
        return self;
    }

    public static Result<Unit, TError> Tap<TError>(this Result<Unit, TError> self, Action action)
    {
        if (self.IsSuccess)
            action();
        return self;
    }

    public static Result<T, TError> TapError<T, TError>(this Result<T, TError> self, Action<TError> action)
    {
        if (self.IsFailure)
            action(self.Error);
        return self;
    }

#if NET9_0_OR_GREATER

    public static RefResult<T, TError> Tap<T, TError>(this RefResult<T, TError> self, Action<T> action)
        where T : allows ref struct where TError : allows ref struct
    {
        if (self.IsSuccess)
            action(self.Value);
        return self;
    }

    public static RefResult<Unit, TError> Tap<TError>(this RefResult<Unit, TError> self, Action action)
        where TError : allows ref struct
    {
        if (self.IsSuccess)
            action();
        return self;
    }

    public static RefResult<T, TError> TapError<T, TError>(this RefResult<T, TError> self, Action<TError> action)
        where T : allows ref struct where TError : allows ref struct
    {
        if (self.IsFailure)
            action(self.Error);
        return self;
    }

#endif
}
