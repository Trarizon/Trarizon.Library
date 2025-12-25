using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<TResult> Zip<T, T2, TResult>(this Optional<T> self, Optional<T2> other, Func<T, T2, TResult> selector)
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

    public static Optional<(T, T2)> Zip<T, T2>(this Optional<T> self, Optional<T2> other)
        => self.HasValue && other.HasValue ? Optional.Of((self.Value, other.Value)) : Optional.None;

#if NET9_0_OR_GREATER

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Zip<T, T2, TResult>(this Optional<T> self, Optional<T2> other, RefFunc<T, T2, TResult> selector)
        where TResult : allows ref struct
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

    public static Optional<TResult> Zip<T, T2, TResult>(this Optional<T> self, RefOptional<T2> other, Func<T, T2, TResult> selector)
        where T2 : allows ref struct
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Zip<T, T2, TResult>(this Optional<T> self, RefOptional<T2> other, RefFunc<T, T2, TResult> selector)
        where T2 : allows ref struct where TResult : allows ref struct
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

    public static Optional<TResult> Zip<T, T2, TResult>(this RefOptional<T> self, Optional<T2> other, Func<T, T2, TResult> selector)
        where T : allows ref struct
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Zip<T, T2, TResult>(this RefOptional<T> self, Optional<T2> other, RefFunc<T, T2, TResult> selector)
        where T : allows ref struct where TResult : allows ref struct
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

    public static Optional<TResult> Zip<T, T2, TResult>(this RefOptional<T> self, RefOptional<T2> other, Func<T, T2, TResult> selector)
        where T : allows ref struct where T2 : allows ref struct
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Zip<T, T2, TResult>(this RefOptional<T> self, RefOptional<T2> other, RefFunc<T, T2, TResult> selector)
        where T : allows ref struct where T2 : allows ref struct where TResult : allows ref struct
        => self.HasValue && other.HasValue ? Optional.Of(selector(self.Value, other.Value)) : Optional.None;

#endif
}
