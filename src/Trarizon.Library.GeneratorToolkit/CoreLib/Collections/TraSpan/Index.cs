using System.Runtime.CompilerServices;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraSpan
{
    public static int OffsetOf<T>(this Span<T> span, ref readonly T item)
        => ((ReadOnlySpan<T>)span).OffsetOf(in item);

    public static int OffsetOf<T>(this ReadOnlySpan<T> span, ref readonly T item)
        => (int)Unsafe.ByteOffset(ref Unsafe.AsRef(in span[0]), ref Unsafe.AsRef(in item)) / Unsafe.SizeOf<T>();
}
