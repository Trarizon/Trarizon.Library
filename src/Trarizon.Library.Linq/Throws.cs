using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Linq;

[StackTraceHidden]
internal static class Throws
{
    [DoesNotReturn]
    public static void IteratorNotSupport([CallerMemberName] string memberName = "")
        => throw new NotSupportedException($"The iterator does not support member '{memberName}'.");

    [DoesNotReturn]
    public static void CollectionIsEmpty(string collectionName) => ThrowInvalidOperation($"{collectionName} is empty.");

    [DoesNotReturn]
    public static void CollectionIsEmpty()
        => ThrowInvalidOperation("Collection must not be empty.");

    public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(value, name);
#else
        if (value < 0)
            ThrowArgumentOutOfRange(name, value, "Value must be a non-negatie value.");
#endif
    }

    public static void ThrowIfGreaterThan(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, name);
#else
        if (value > other)
            ThrowArgumentOutOfRange(name, value, $"Value must be less than or equal to '{other}'.");
#endif
    }

    public static void ThrowIfIndexGreaterThanOrEqual(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)value, (uint)other, name);
#else
        if ((uint)value >= (uint)other)
            ThrowArgumentOutOfRange(name, value, $"Value must be positive and less than '{other}'.");
#endif
    }

    #region Exception

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRange(string? paramName, object? value, string? message)
        => throw new ArgumentOutOfRangeException(paramName, value, message);

    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    #endregion
}
