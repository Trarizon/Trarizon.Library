using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
partial class TraSpan
{
    public static Span<byte> AsBytes<T>(ref T value) where T : unmanaged
        => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>());

    public static ReadOnlySpan<byte> AsReadOnlyBytes<T>(ref readonly T value) where T : unmanaged
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in value)), Unsafe.SizeOf<T>());
}
