using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library;
internal static partial class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
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

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRange(string? message = null, object? value = null, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => throw new ArgumentOutOfRangeException(message, value, paramName);

    [DoesNotReturn]
    public static void Throw(Exception exception)
        => throw exception;
}
