#if NETSTANDARD

using SysUnsafe = System.Runtime.CompilerServices.Unsafe;

namespace Trarizon.Library.Netstd;
internal static class NetstdFix_Unsafe
{
    public static bool IsNullRef<T>(ref readonly T reference) => SysUnsafe.IsNullRef(ref System.Runtime.CompilerServices.Unsafe.AsRef(in reference));

    public static ref T NullRef<T>() => ref SysUnsafe.NullRef<T>();

    public static bool AreSame<T>(ref readonly T left, ref readonly T right) => SysUnsafe.AreSame(ref SysUnsafe.AsRef(in left), ref SysUnsafe.AsRef(in right));

    public static nint ByteOffset<T>(ref readonly T origin, ref readonly T target) => SysUnsafe.ByteOffset(ref SysUnsafe.AsRef(in origin), ref SysUnsafe.AsRef(in target));
}

internal static class NetstdFix_MemoryMarshal
{
    public static unsafe Span<T> CreateSpan<T>(ref T reference, int length) where T : unmanaged
    {
        fixed (T* ptr = &reference) {
            return new Span<T>(ptr, length);
        }
    }

    public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length) where T : unmanaged
    {
        fixed (T* ptr = &reference) {
            return new ReadOnlySpan<T>(ptr, length);
        }
    }
}

#endif
