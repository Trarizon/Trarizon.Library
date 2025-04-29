using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Helpers;
[StackTraceHidden]
internal static class Throws
{
    [DoesNotReturn]
    public static void CollectionModifiedAfterEnumeratorCreated()
        => throw new InvalidOperationException("Collection has been modified after enumerator created.");

    [DoesNotReturn]
    public static void IteratorIsImmutable()
        => throw new InvalidOperationException("Iterator is immutable.");

    [DoesNotReturn]
    public static void CollectionHasNoElement()
        => throw new InvalidOperationException("Collection has no element.");

    [DoesNotReturn]
    public static void NodeNotBelongsToCollection()
        => throw new InvalidOperationException("Node does not belong to this collection.");

    [DoesNotReturn]
    public static void NodeIsInvalidated()
        => throw new InvalidOperationException("The node is invalidated.");

    [DoesNotReturn]
    internal static void KeyNotFound<T>(T key)
        => throw new KeyNotFoundException($"Cannot find key '{key}' in collection.");

}
