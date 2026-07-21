namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<T> Where<T>(this Optional<T> self, Func<T, bool> predicate)
        => self.HasValue && predicate(self.Value) ? self : Optional.None;

    public static Optional<T> OfNotNull<T>(this Optional<T?> value) where T : class
        => value.HasValue && value.Value is { } v ? new(v) : default;

    public static Optional<T> OfNotNull<T>(this Optional<T?> value) where T : struct
        => value.HasValue && value.Value is { } v ? new(v) : default;

#if REF_MONAD

    public static RefOptional<T> Where<T>(this RefOptional<T> self, Func<T, bool> predicate)
        where T : allows ref struct
        => self.HasValue && predicate(self.Value) ? self : Optional.None;

#endif

    #region With State

    public static Optional<T> Where<T, TState>(this Optional<T> self, TState state, Func<T, TState, bool> predicate)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
        => self.HasValue && predicate(self.Value, state) ? self : Optional.None;

#if REF_MONAD

    public static RefOptional<T> Where<T, TState>(this RefOptional<T> self, TState state, Func<T, TState, bool> predicate)
        where T : allows ref struct where TState : allows ref struct
        => self.HasValue && predicate(self.Value, state) ? self : Optional.None;

#endif

    #endregion
}
