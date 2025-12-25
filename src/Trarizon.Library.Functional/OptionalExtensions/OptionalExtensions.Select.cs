using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<TResult> Select<T, TResult>(this Optional<T> self, Func<T, TResult> selector)
        => self.HasValue ? Optional.Of(selector(self.Value)) : Optional.None;

#if NET9_0_OR_GREATER

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Select<T, TResult>(this Optional<T> self, RefFunc<T, TResult> selector)
        where TResult : allows ref struct
        => self.HasValue ? Optional.Of(selector(self.Value)) : Optional.None;

    public static Optional<TResult> Select<T, TResult>(this RefOptional<T> self, Func<T, TResult> selector)
        where T : allows ref struct
        => self.HasValue ? Optional.Of(selector(self.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Select<T, TResult>(this RefOptional<T> self, RefFunc<T, TResult> selector)
        where T : allows ref struct where TResult : allows ref struct
        => self.HasValue ? Optional.Of(selector(self.Value)) : Optional.None;

#endif
}
