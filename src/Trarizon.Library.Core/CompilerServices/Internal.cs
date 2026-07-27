using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.CompilerServices;

internal static class Internal
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReferenceAt<T>(ReadOnlySpan<T> span, int index)
        => ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index);
}
