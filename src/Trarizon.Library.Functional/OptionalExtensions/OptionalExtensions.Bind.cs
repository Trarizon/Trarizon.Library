using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<TResult> Bind<T, TResult>(this Optional<T> self, Func<T, Optional<TResult>> selector)
        => self.HasValue ? selector(self.Value) : Optional.None;

#if REF_MONAD

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

    #region With State

    public static Optional<TResult> Bind<T, TState, TResult>(this Optional<T> self, TState state, Func<T, TState, Optional<TResult>> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
        => self.HasValue ? selector(self.Value, state) : Optional.None;

#if REF_MONAD

    public static RefOptional<TResult> Bind<T, TState, TResult>(this Optional<T> self, TState state, Func<T, TState, RefOptional<TResult>> selector)
        where TState : allows ref struct where TResult : allows ref struct
        => self.HasValue ? selector(self.Value, state) : Optional.None;

    public static Optional<TResult> Bind<T, TState, TResult>(this RefOptional<T> self, TState state, Func<T, TState, Optional<TResult>> selector)
        where T : allows ref struct where TState : allows ref struct
        => self.HasValue ? selector(self.Value, state) : Optional.None;

    public static RefOptional<TResult> Bind<T, TState, TResult>(this RefOptional<T> self, TState state, Func<T, TState, RefOptional<TResult>> selector)
        where T : allows ref struct where TState : allows ref struct where TResult : allows ref struct
        => self.HasValue ? selector(self.Value, state) : Optional.None;

#endif

    #endregion

    #region SelectMany

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Optional<TResult> SelectMany<T, TResult>(this Optional<T> self, Func<T, Optional<TResult>> selector)
        => self.Bind(selector);

#if REF_MONAD

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
}
