using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Linq.Helpers;
internal static partial class Throws
{
    [DoesNotReturn]
    public static void IteratorNotSupport([CallerMemberName] string memberName = "") 
        => throw new NotSupportedException($"Iterator doesn't support member '{memberName}'.");

    [DoesNotReturn]
    public static void CollectionIsEmpty()
        => ThrowInvalidOperation("Collection must not be empty.");

    [DoesNotReturn]
    public static void ThrowInvalidOperation(string? message = null)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    public static void ThrowArgumentOutOfRange(string? paramName, object? value, string? message)
        => throw new ArgumentOutOfRangeException(paramName, value, message);
}
