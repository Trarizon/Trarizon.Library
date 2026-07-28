using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library;

#if NET6_0_OR_GREATER
[System.Diagnostics.StackTraceHidden]
#endif
internal static partial class Throws
{
    [DoesNotReturn]
    public static T UnknownEnumCase<T>(Enum value)
    {
        ThrowInvalidOperation($"Unknown enum value '{value}'");
        return default!;
    }

    [DoesNotReturn]
    public static void KeyNotFound(object key, string collectionName)
        => ThrowKeyNotFound($"The given key '{key}' is not present in {collectionName}.");

    [DoesNotReturn]
    public static void KeyNotFound<T>(ReadOnlySpan<T> key, string collectionName)
        => ThrowKeyNotFound($"The given key '{key.ToString()}' is not present in {collectionName}.");

    [DoesNotReturn]
    public static void KeyAlreadyExists(object key, string collectionName, [CallerArgumentExpression(nameof(key))] string paramName = "")
        => ThrowArgument($"Key '{key}' is already existing in collection.", paramName);

    [DoesNotReturn]
    public static void CollectionIsEmpty(string collectionName) => ThrowInvalidOperation($"{collectionName} is empty.");

    [DoesNotReturn]
    public static void CollectionModifiedDuringEnumeration()
        => throw new InvalidOperationException("Collection was modified during enumeration.");

    [DoesNotReturn]
    public static void IncompatibleAlternateComparer()
        => ThrowInvalidOperation("Incompatible alternate comparer");

}
