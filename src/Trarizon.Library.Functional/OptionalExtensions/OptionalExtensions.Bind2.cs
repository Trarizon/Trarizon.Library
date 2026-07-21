using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    // No 2-parameter Bind overload, we provide the SelectMany to make linq expression available, but manually call Bind wont need it.
    // Developers can use .Select(x => (x, y)).Bind(tpl => Do(tpl.x, tpl.y)) as workaround

    #region SelectMany

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

#if REF_MONAD

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, Optional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this Optional<T> self, Func<T, RefOptional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where T : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, Optional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TMid : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

    [OverloadResolutionPriority(-1)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static RefOptional<TResult> SelectMany<T, TMid, TResult>(this RefOptional<T> self, Func<T, RefOptional<TMid>> selector, RefFunc<T, TMid, TResult> resultSelector)
        where T : allows ref struct where TMid : allows ref struct where TResult : allows ref struct
        => self.HasValue && selector(self.Value) is { HasValue: true } mid ? Optional.Of(resultSelector(self.Value, mid.Value)) : Optional.None;

#endif

    #endregion
}
