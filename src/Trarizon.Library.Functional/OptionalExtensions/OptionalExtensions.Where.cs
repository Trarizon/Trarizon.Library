namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<T> Where<T>(this Optional<T> self, Func<T, bool> predicate)
        => self.HasValue && predicate(self.Value) ? self : Optional.None;

    public static Optional<T> OfNotNull<T>(this Optional<T?> value) where T : class
        => value.HasValue && value.Value is { } v ? new(v) : default;

    public static Optional<T> OfNotNull<T>(this Optional<T?> value) where T : struct
        => value.HasValue && value.Value is { } v ? new(v) : default;

#if NET9_0_OR_GREATER

    public static RefOptional<T> Where<T>(this RefOptional<T> self, Func<T, bool> predicate)
        where T : allows ref struct
        => self.HasValue && predicate(self.Value) ? self : Optional.None;

#endif
}
