using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;

#if NETSTANDARD

internal static partial class Polyfills
{
    extension(MemoryMarshal)
    {
        public static Span<T> AsSpan<T>(List<T> list)
            => TraCollection.UnsafeAccess<T>.GetItems(list).AsSpan(0, list.Count);
    }
}

#endif
