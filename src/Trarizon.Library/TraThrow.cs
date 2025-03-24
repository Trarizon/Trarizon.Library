using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library;
[StackTraceHidden]
internal static class TraThrow
{
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
    internal static void NodeBelongsWrong() 
        => throw new InvalidOperationException("Node does not belong to this collection.");

    [DoesNotReturn]
    internal static void Exception(Exception exception)
        => throw exception;

    [DoesNotReturn]
    internal static void IteratorImmutable()
        => throw new InvalidOperationException("Iterator is immutable");

    [DoesNotReturn]
    public static void InvalidEnumState(Enum state)
        => throw new InvalidOperationException($"Invalid enum state: {state}");

    [DoesNotReturn]
    public static T InvalidEnumState<T>(Enum state)
        => throw new InvalidOperationException($"Invalid enum state: {state}");
}
