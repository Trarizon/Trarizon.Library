namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static Optional<T> Tap<T>(this Optional<T> self, Action<T> action)
    {
        if (self.HasValue)
            action(self.Value);
        return self;
    }

#if NET9_0_OR_GREATER

    public static RefOptional<T> Tap<T>(this RefOptional<T> self, Action<T> action) where T : allows ref struct
    {
        if (self.HasValue)
            action(self.Value);
        return self;
    }

#endif
}
