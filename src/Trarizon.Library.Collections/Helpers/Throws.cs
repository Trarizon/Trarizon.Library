using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
[StackTraceHidden]
internal static partial class Throws
{
    #region Condition

    public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(value, name);
#else
        if (value < 0)
            ThrowArgumentOutOfRange(name, value, "Value must be a non-negatie value.");
#endif
    }

    public static void ThrowIfNegativeOrZero(int value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, name);
#else
        if (value <= 0)
            ThrowArgumentOutOfRange(name, value, "Value must be a positive value.");
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

    public static void ThrowIfLessThan(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThan(value, other, name);
#else
        if (value < other)
            ThrowArgumentOutOfRange(name, value, $"Value must be greater than or equal to '{other}'.");
#endif
    }

    public static void ThrowIfGreaterThanOrEqual(int value, int other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, name);
#else
        if (value >= other)
            ThrowArgumentOutOfRange(name, value, $"Value must be less than '{other}'.");
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

    public static void ThrowIfGreaterThan(uint value, uint other, [CallerArgumentExpression(nameof(value))] string name = "")
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, name);
#else
        if (value > other)
            ThrowArgumentOutOfRange(name, value, $"Value must be less than or equal to '{other}'.");
#endif
    }

    #endregion

    #region Exception

    [DoesNotReturn]
    private static void ThrowArgumentOutOfRange(string? paramName, object? value, string? message)
        => throw new ArgumentOutOfRangeException(paramName, value, message);

    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    private static void ThrowArgument(string? message = null, string? paramName = null)
        => throw new ArgumentException(message, paramName);

    [DoesNotReturn]
    private static void ThrowKeyNotFound(string? message = null)
        => throw new KeyNotFoundException(message);

    [DoesNotReturn]
    public static void ThrowNotSupport(string? message = null)
        => throw new NotSupportedException(message);

    #endregion

    [DoesNotReturn]
    public static void CollectionModifiedDuringEnumeration()
        => throw new InvalidOperationException("Collection was modified during enumeration.");

    [DoesNotReturn]
    public static void CollectionIsReadOnly()
        => ThrowInvalidOperation("Collection is read only.");

    [DoesNotReturn]
    public static void CollectionIsEmpty(string collectionName) => ThrowInvalidOperation($"{collectionName} is empty.");

    [DoesNotReturn]
    public static void KeyNotFound(object key, string collectionName)
        => ThrowKeyNotFound($"The given key '{key}' is not present in {collectionName}.");

    [DoesNotReturn]
    public static void KeyAlreadyExists(object key, string collectionName, [CallerArgumentExpression(nameof(key))] string paramName = "")
        => ThrowArgument($"Key '{key}' is already existing in collection.", paramName);

    [DoesNotReturn]
    public static void IndexArgOutOfRange(int index, [CallerArgumentExpression(nameof(index))] string paramName = "")
        => ThrowArgumentOutOfRange(paramName, index, null);
    [DoesNotReturn]
    public static void IncompatibleAlternateComparer()
        => ThrowInvalidOperation("Incompatible alternate comparer");
}
