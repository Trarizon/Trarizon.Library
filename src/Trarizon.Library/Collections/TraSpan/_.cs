using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static Span<T> UnsafeCast<T>(Span<byte> bytes)
    {
        ref byte first = ref MemoryMarshal.GetReference(bytes);
        ref T firstRef = ref Unsafe.As<byte, T>(ref first);
        return MemoryMarshal.CreateSpan(ref firstRef, bytes.Length / Unsafe.SizeOf<T>());
    }

    public static Span<T> UnsafeCast<T>(Span<nint> nints) where T : class
    {
        ref nint first = ref MemoryMarshal.GetReference(nints);
        ref T firstRef = ref Unsafe.As<nint, T>(ref first);
        return MemoryMarshal.CreateSpan(ref firstRef, nints.Length);
    }

    public static Span<T> AsSpan<T>(ref T value) => MemoryMarshal.CreateSpan(ref value, 1);

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(ref readonly T value) => MemoryMarshal.CreateReadOnlySpan(in value, 1);
}
