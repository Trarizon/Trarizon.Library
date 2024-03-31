using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Helpers;
public static class StreamHelper
{
    public static unsafe T Read<T>(this Stream stream) where T : unmanaged
    {
        Span<byte> bfr = stackalloc byte[sizeof(T)];
        stream.Read(bfr);
        return Unsafe.ReadUnaligned<T>(in MemoryMarshal.GetReference(bfr));
    }
}
