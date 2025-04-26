using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => ((ReadOnlySpan<T>)span).OffsetOf(in item);

    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ref readonly T item)
        => (int)Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref Unsafe.AsRef(in item)) / Unsafe.SizeOf<T>();

    public static int OffsetOf<T>(this Span<T> span, ReadOnlySpan<T> subSpan)
        => (int)Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(subSpan)) / Unsafe.SizeOf<T>();

    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> subSpan)
        => (int)Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(subSpan)) / Unsafe.SizeOf<T>();

    /// <summary>
    /// Get index of <paramref name="item"/> by byte offset. This method allows pass a different type of 
    /// <paramref name="item"/> so you can pass a ref of a field of a <typeparamref name="TSpan"/> value
    /// </summary>
    public static int DangerousOffsetOf<TSpan, TItem>(this ReadOnlySpan<TSpan> span, ref readonly TItem item) where TSpan : struct
        => (int)Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref Unsafe.As<TItem, TSpan>(ref Unsafe.AsRef(in item))) / Unsafe.SizeOf<TSpan>();
}
