using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library;
internal static partial class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void ThrowNotSupport(string? message = null)
        => throw new NotSupportedException(message);

    [DoesNotReturn]
    public static void ThrowKeyNotFound(string key)
        => throw new KeyNotFoundException($"key '{key}' not found");

    [DoesNotReturn]
    public static void ThrowArgument(string? message = null, string? paramName = null)
        => throw new ArgumentException(message, paramName);
}
