namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static TResult Match<T, TResult>(this Optional<T> self, Func<T, TResult> selector, Func<TResult> noValueSelector)
#if NET9_0_OR_GREATER
        where TResult : allows ref struct
#endif
        => self.HasValue ? selector(self.Value) : noValueSelector();

    public static void Match<T>(this Optional<T> self, Action<T>? selector, Action? noneSelector)
    {
        if (self.HasValue)
            selector?.Invoke(self.Value);
        else
            noneSelector?.Invoke();
    }

    public static void MatchValue<T>(this Optional<T> self, Action<T> selector)
    {
        if (self.HasValue) selector(self.Value);
    }

    public static void MatchNone<T>(this Optional<T> self, Action selector)
    {
        if (!self.HasValue) selector();
    }

#if NET9_0_OR_GREATER

    public static TResult Match<T, TResult>(this RefOptional<T> self, Func<T, TResult> selector, Func<TResult> noValueSelector)
        where T : allows ref struct where TResult : allows ref struct
        => self.HasValue ? selector(self.Value) : noValueSelector();

    public static void Match<T>(this RefOptional<T> self, Action<T>? selector, Action? noneSelector)
        where T : allows ref struct
    {
        if (self.HasValue)
            selector?.Invoke(self.Value);
        else
            noneSelector?.Invoke();
    }

    public static void MatchValue<T>(this RefOptional<T> self, Action<T> selector)
        where T : allows ref struct
    {
        if (self.HasValue) selector(self.Value);
    }

    public static void MatchNone<T>(this RefOptional<T> self, Action selector)
        where T : allows ref struct
    {
        if (!self.HasValue) selector();
    }

#endif
}
