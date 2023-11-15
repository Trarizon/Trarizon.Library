using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library;
internal static partial class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRange(string? paramName, string? message = null)
        => throw new ArgumentOutOfRangeException(paramName, message);

    [DoesNotReturn]
    public static void ThrowNotSupport(string? message = null)
        => throw new NotSupportedException(message);
}
