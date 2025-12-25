namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<T> Or<T>(Optional.NoneBuilder self, Optional<T> other)
        => other;

    public static Optional<T> Or<T>(Optional.NoneBuilder self, Func<Optional<T>> otherSelector)
        => otherSelector();

    public static Optional<T> Or<T>(this Optional<T> self, Optional<T> other)
        => self.HasValue ? self : other;

    public static Optional<T> Or<T>(this Optional<T> self, Func<Optional<T>> otherSelector)
        => self.HasValue ? self : otherSelector();

#if NET9_0_OR_GREATER

    public static RefOptional<T> Or<T>(Optional.NoneBuilder self, RefOptional<T> other)
        where T : allows ref struct
        => other;

    public static RefOptional<T> Or<T>(Optional.NoneBuilder self, Func<RefOptional<T>> otherSelector)
        where T : allows ref struct
        => otherSelector();

    public static RefOptional<T> Or<T>(this RefOptional<T> self, RefOptional<T> other)
        where T : allows ref struct
        => self.HasValue ? self : other;

    public static RefOptional<T> Or<T>(this RefOptional<T> self, Func<RefOptional<T>> otherSelector)
        where T : allows ref struct
        => self.HasValue ? self : otherSelector();

#endif
}
