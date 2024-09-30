using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.CoreLib;
internal static class TraThrow
{
    [DoesNotReturn]
    public static void KeyNotFound<T>(T key)
        => throw new KeyNotFoundException($"Cannot find key '{key}' in collection.");

    [DoesNotReturn]
    public static void CollectionModified()
        => throw new InvalidOperationException("Collection has been modified.");

    [DoesNotReturn]
    public static void NoElement()
        => throw new InvalidOperationException("Collection has no element.");

    [DoesNotReturn]
    public static void Exception(Exception exception)
        => throw exception;
}
