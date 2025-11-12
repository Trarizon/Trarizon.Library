using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;

#if NETSTANDARD2_0

internal static class Polyfills
{
    extension(RuntimeHelpers)
    {
        public static bool IsReferenceOrContainsReferences<T>()
        {
            return !typeof(T).IsPrimitive;
        }
    }

    extension(MemoryMarshal)
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
}
#endif
