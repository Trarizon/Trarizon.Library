namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static T? ToNullable<T>(this Optional<T> optional) where T : struct
        => optional.HasValue ? optional.Value : null;

#if REF_MONAD

    public static T? ToNullable<T>(this RefOptional<T> optional) where T : struct
        => optional.HasValue ? optional.Value : null;

#endif
}
