using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
partial class TraSpan
{
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => ((ReadOnlySpan<T>)span).OffsetOf(in item);

    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ref readonly T item)
        => (int)Unsafe.ByteOffset(in MemoryMarshal.GetReference(span), in item) / Unsafe.SizeOf<T>();
}
