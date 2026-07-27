using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library;

internal static partial class Throws
{
    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void ThrowNotSupport(string? message = null)
        => throw new NotSupportedException(message);

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRange(string? paramName, object? value, string? message)
        => throw new ArgumentOutOfRangeException(paramName, value, message);

}
