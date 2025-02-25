using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETSTANDARD2_0
using MemoryMarshal = Trarizon.Library.Netstd.NetstdFix_MemoryMarshal;
#endif

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static Span<byte> AsBytes<T>(ref T value) where T : unmanaged
        => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>());

    public static ReadOnlySpan<byte> AsReadOnlyBytes<T>(ref readonly T value) where T : unmanaged
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in value)), Unsafe.SizeOf<T>());
}
