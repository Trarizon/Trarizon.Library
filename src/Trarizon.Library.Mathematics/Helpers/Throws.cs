using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Mathematics.Helpers;
internal static class Throws
{
    public static void ThrowIfGreaterThan(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, name);
#else
        if (value > other)
            ThrowArgumentOutOfRange(name, value, $"Value must be less than or equal to '{other}'.");
#endif
    }

    public static void ThrowIfGreaterThan(float value, float other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, name);
#else
        if (value > other)
            ThrowArgumentOutOfRange(name, value, $"Value must be less than or equal to '{other}'.");
#endif
    }

    public static void ThrowIfGreaterThan(double value, double other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, name);
#else
        if (value > other)
            ThrowArgumentOutOfRange(name, value, $"Value must be less than or equal to '{other}'.");
#endif
    }

    [DoesNotReturn]
    public static void ThrowArgument(string? message = null, string? paramName = null)
        => throw new ArgumentException(message, paramName);

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRange(string? paramName, object? value, string? message)
        => throw new ArgumentOutOfRangeException(paramName, value, message);
    
    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static T ThrowUnreachable<T>()
    {
#if NET8_0_OR_GREATER
        throw new UnreachableException();
#else
        ThrowInvalidOperation("Unreachable");
        return default!;
#endif
    }
}
