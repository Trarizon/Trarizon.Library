using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library;
[StackTraceHidden]
public static class TraThrow
{
    [DoesNotReturn]
    public static void ThrowSwitchExpressionException(object? unmatchedValue = null)
        => throw new SwitchExpressionException(unmatchedValue);

    [DoesNotReturn]
    public static T ThrowSwitchExpressionException<T>(object? unmatchedValue = null)
        => throw new SwitchExpressionException(unmatchedValue);

    [DoesNotReturn]
    internal static void KeyNotFound<T>(T key)
        => throw new KeyNotFoundException($"Cannot find key '{key}' in collection.");

    [DoesNotReturn]
    internal static void CollectionModified()
        => throw new InvalidOperationException("Collection has been modified.");

    [DoesNotReturn]
    internal static void NoElement()
        => throw new InvalidOperationException("Collection has no element.");

    [DoesNotReturn]
    internal static void Exception(Exception exception)
        => throw exception;

    [DoesNotReturn]
    internal static void IteratorImmutable()
        => throw new InvalidOperationException("Iterator is immutable");
}
