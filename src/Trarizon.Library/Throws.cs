using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library;
internal static class Throws
{
    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void ThrowNotSupport(string? message = null)
        => throw new NotSupportedException(message);

    [DoesNotReturn]
    public static T UnknownEnumCase<T>(Enum value)
    {
        ThrowInvalidOperation($"Unknown enum value '{value}'");
        return default!;
    }
}
