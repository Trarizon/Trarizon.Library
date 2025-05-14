using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Mathematics.Helpers;
#if NETSTANDARD
internal static class PfBitOperations
{
    private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => // 32
    [
        00, 01, 28, 02, 29, 14, 24, 03,
        30, 22, 20, 15, 25, 17, 04, 08,
        31, 27, 13, 23, 21, 19, 16, 07,
        26, 12, 18, 06, 11, 05, 10, 09
    ];

    public static int TrailingZeroCount(int value) => TrailingZeroCount((uint)value);

    public static int TrailingZeroCount(long value) => TrailingZeroCount((ulong)value);

    public static int TrailingZeroCount(uint value)
    {
        if (value == 0)
            return 32;
        return Unsafe.AddByteOffset(
            ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
            (IntPtr)(int)(((value & (uint)-(int)value) * 0x077CB531u) >> 27));
    }

    public static int TrailingZeroCount(ulong value)
    {
        uint low = (uint)value;
        if (low == 0) {
            return 32 + TrailingZeroCount((uint)(value >> 32));
        }
        return TrailingZeroCount(low);
    }
}

#endif