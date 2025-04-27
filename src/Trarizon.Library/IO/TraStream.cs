using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.IO;
public static class TraStream
{
    /// <summary>
    /// Read a sequence of datas of type <typeparamref name="T"/> from stream
    /// </summary>
    /// <returns>
    /// The number of <typeparamref name="T"/> read into buffer
    /// </returns>
    public static int Read<T>(this Stream stream, Span<T> buffer) where T : unmanaged
    {
        var bytes = MemoryMarshal.AsBytes(buffer);
        var read = stream.Read(bytes);
        return read / Unsafe.SizeOf<T>();
    }

    public static void ReadExactly<T>(this Stream stream, Span<T> buffer) where T : unmanaged
    {
        var bytes = MemoryMarshal.AsBytes(buffer);
        stream.ReadExactly(bytes);
    }

    public static T[] ReadExactlyIntoArray<T>(this Stream stream, int length) where T : unmanaged
    {
        T[] arr = new T[length];
        stream.ReadExactly(arr.AsSpan());
        return arr;
    }

    /// <summary>
    /// Read a <see cref="int"/> as byte array length, and read bytes in given
    /// length, cast it into <typeparamref name="T"/>
    /// </summary>
    public static T[] ReadWithInt32Prefix<T>(this Stream stream) where T : unmanaged
    {
        var bfr = (stackalloc byte[sizeof(int)]);
        stream.ReadExactly(bfr);
        var len = MemoryMarshal.Read<int>(bfr);
        if (len <= 0)
            return [];

        return stream.ReadExactlyIntoArray<T>(len / Unsafe.SizeOf<T>());
    }
}
