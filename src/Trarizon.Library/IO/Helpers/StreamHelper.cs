using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.IO.Helpers;
public static class StreamHelper
{
    public static T Read<T>(this Stream stream) where T : unmanaged
    {
        Span<byte> bfr = stackalloc byte[Unsafe.SizeOf<T>()];
        stream.Read(bfr);
        return Unsafe.ReadUnaligned<T>(in MemoryMarshal.GetReference(bfr));
    }
}
