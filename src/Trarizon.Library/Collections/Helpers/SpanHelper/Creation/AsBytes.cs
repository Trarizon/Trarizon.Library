using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.Library.Helpers;

namespace Trarizon.Library.Collections.Helpers;
partial class SpanHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> AsBytes<T>(ref T value) where T : unmanaged
    {
        return MemoryMarshal.CreateSpan(
            ref Unsafe.As<T, byte>(ref value),
            Unsafe.SizeOf<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> AsReadOnlyBytes<T>(ref readonly T value) where T : unmanaged
    {
        return MemoryMarshal.CreateReadOnlySpan(
            in UnsafeHelper.AsReadOnly<T, byte>(in value),
            Unsafe.SizeOf<T>());
    }
}
