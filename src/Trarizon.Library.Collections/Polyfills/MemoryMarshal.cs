using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;

#if NETSTANDARD

internal static partial class Polyfills
{
    extension(MemoryMarshal)
    {
#pragma warning disable CS8500

#if NETSTANDARD2_0

        public static unsafe Span<T> CreateSpan<T>(scoped ref T reference, int length)
        {
            fixed (T* ptr = &reference)
            {
                return new Span<T>(ptr, length);
            }
        }

        public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(scoped ref readonly T reference, int length)
        {
            fixed (T* ptr = &reference)
            {
                return new ReadOnlySpan<T>(ptr, length);
            }
        }

#endif

#pragma warning restore CS8500

        public static Span<T> AsSpan<T>(List<T> list)
            => TraCollection.UnsafeAccess<T>.GetItems(list).AsSpan(0, list.Count);
    }
}

#endif
