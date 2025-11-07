using System.Collections;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;

#if NETSTANDARD

internal static class Polyfills
{
    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> collection) {
            count = collection.Count;
            return true;
        }
        if (source is ICollection ngcollection) {
            count = ngcollection.Count;
            return true;
        }
        count = 0;
        return false;
    }

}

#endif

#if NETSTANDARD2_0
internal static class PfRuntimeHelpers
{
    public static bool IsReferenceOrContainsReferences<T>()
    {
        return !typeof(T).IsPrimitive;
    }
}

internal static class PfMemoryMarshal
{
#pragma warning disable CS8500

    public static unsafe Span<T> CreateSpan<T>(scoped ref T reference, int length)
    {
        fixed (T* ptr = &reference) {
            return new Span<T>(ptr, Unsafe.SizeOf<T>());
        }
    }

    public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(scoped ref readonly T reference, int length)
    {
        fixed (T* ptr = &reference) {
            return new ReadOnlySpan<T>(ptr, Unsafe.SizeOf<T>());
        }
    }

#pragma warning restore CS8500
}
#endif
