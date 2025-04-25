using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETSTANDARD
using Unsafe = Trarizon.Library.Netstd.NetstdFix_Unsafe;
#endif

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => ((ReadOnlySpan<T>)span).OffsetOf(in item);

    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ref readonly T item)
        => (int)Unsafe.ByteOffset(in MemoryMarshal.GetReference(span), in item) / System.Runtime.CompilerServices.Unsafe.SizeOf<T>();

    public static int OffsetOf<T>(this Span<T> span, ReadOnlySpan<T> subSpan)
        => (int)Unsafe.ByteOffset(in MemoryMarshal.GetReference(span), in MemoryMarshal.GetReference(subSpan)) / System.Runtime.CompilerServices.Unsafe.SizeOf<T>();

    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> subSpan)
        => (int)Unsafe.ByteOffset(in MemoryMarshal.GetReference(span), in MemoryMarshal.GetReference(subSpan)) / System.Runtime.CompilerServices.Unsafe.SizeOf<T>();

    /// <summary>
    /// Get index of <paramref name="item"/> by byte offset. This method allows pass a different type of 
    /// <paramref name="item"/> so you can pass a ref of a field of a <typeparamref name="TSpan"/> value
    /// </summary>
    public static int DangerousOffsetOf<TSpan, TItem>(this ReadOnlySpan<TSpan> span, ref readonly TItem item) where TSpan : struct
        => (int)Unsafe.ByteOffset(in MemoryMarshal.GetReference(span), in TraUnsafe.AsReadOnly<TItem, TSpan>(in item)) / System.Runtime.CompilerServices.Unsafe.SizeOf<TSpan>();
}
