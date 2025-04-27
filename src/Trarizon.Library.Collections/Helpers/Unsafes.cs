using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
internal static class Unsafes
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReferenceAt<T>(ReadOnlySpan<T> span, int index)
        => ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index);
}
