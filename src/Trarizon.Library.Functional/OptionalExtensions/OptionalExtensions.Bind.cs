using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<TResult> Bind<T, TResult>(this Optional<T> self, Func<T, Optional<TResult>> selector)
        => self.HasValue ? selector(self.Value) : Optional.None;

#if NET9_0_OR_GREATER

    public static RefOptional<TResult> Bind<T, TResult>(this Optional<T> self, Func<T, RefOptional<TResult>> selector)
        where TResult : allows ref struct
        => self.HasValue ? selector(self.Value) : Optional.None;

    public static Optional<TResult> Bind<T, TResult>(this RefOptional<T> self, Func<T, Optional<TResult>> selector)
        where T : allows ref struct
        => self.HasValue ? selector(self.Value) : Optional.None;

    public static RefOptional<TResult> Bind<T, TResult>(this RefOptional<T> self, Func<T, RefOptional<TResult>> selector)
        where T : allows ref struct where TResult : allows ref struct
        => self.HasValue ? selector(self.Value) : Optional.None;

#endif

    #region SelectMany

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TResult>(this Optional<T> self, Func<T, Optional<TResult>> selector)
        => self.Bind(selector);

#if NET9_0_OR_GREATER

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TResult>(this Optional<T> self, Func<T, RefOptional<TResult>> selector)
        where TResult : allows ref struct
        => self.Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TResult>(this RefOptional<T> self, Func<T, Optional<TResult>> selector)
        where T : allows ref struct
        => self.Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TResult>(this RefOptional<T> self, Func<T, RefOptional<TResult>> selector)
        where T : allows ref struct where TResult : allows ref struct
        => self.Bind(selector);

#endif

    #endregion

    public static Optional<TResult> Bind<T, TMid, TResult>(this Optional<T> self, Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

#if NET9_0_OR_GREATER

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Bind<T, TMid, TResult>(this Optional<T> self, Func<T, Optional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    public static Optional<TResult> Bind<T, TMid, TResult>(this Optional<T> self, Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Bind<T, TMid, TResult>(this Optional<T> self, Func<T, RefOptional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    public static Optional<TResult> Bind<T, TMid, TResult>(this RefOptional<T> self, Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where T : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Bind<T, TMid, TResult>(this RefOptional<T> self, Func<T, Optional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    public static Optional<TResult> Bind<T, TMid, TResult>(this RefOptional<T> self, Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TMid : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    public static RefOptional<TResult> Bind<T, TMid, TResult>(this RefOptional<T> self, Func<T, RefOptional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TMid : allows ref struct where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

#endif

    #region Select Many

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        => self.Bind(selector, resultSelector);

#if NET9_0_OR_GREATER

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, Optional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where TResult : allows ref struct
        => self.Bind(selector, resultSelector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct
        => self.Bind(selector, resultSelector);

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, RefOptional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct where TResult : allows ref struct
        => self.Bind(selector, resultSelector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where T : allows ref struct
        => self.Bind(selector, resultSelector);

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, Optional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TResult : allows ref struct
        => self.Bind(selector, resultSelector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TMid : allows ref struct
        => self.Bind(selector, resultSelector);

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, RefOptional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TMid : allows ref struct where TResult : allows ref struct
        => self.Bind(selector, resultSelector);

#endif

    #endregion
}
