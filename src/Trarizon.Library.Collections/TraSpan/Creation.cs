using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{

    public static unsafe Span<byte> AsBytes<T>(ref T value) where T : unmanaged
    {
#if NETSTANDARD2_0
        fixed (T* ptr = &value) {
            return new Span<byte>(ptr, sizeof(T));
        }
#else
        return MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>());
#endif
    }

    public static unsafe ReadOnlySpan<byte> AsReadOnlyBytes<T>(ref readonly T value) where T : unmanaged
    {
#if NETSTANDARD2_0
        fixed (T* ptr = &value) {
            return new ReadOnlySpan<byte>(ptr, sizeof(T));
        }
#else
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in value)), Unsafe.SizeOf<T>());
#endif
    }
}
