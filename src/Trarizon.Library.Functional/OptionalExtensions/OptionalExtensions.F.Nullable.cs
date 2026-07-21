namespace Trarizon.Library.Functional;

public static partial class OptionalExtensions
{
    public static T? ToNullable<T>(this in Optional<T> optional) where T : struct
        => optional.HasValue ? optional.Value : null;
}
