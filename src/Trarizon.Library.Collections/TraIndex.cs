using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static class TraIndex
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FlipNegative(ref int index)
    {
        if (index < 0)
            index = ~index;
    }

    /// <summary>
    /// <see cref="Index.GetOffset(int)"/>, and check if the offset is in [0, <paramref name="length"/>),
    /// throw if out of range
    /// </summary>
    public static int GetCheckedOffset(this Index index, int length)
    {
        var offset = index.GetOffset(length);
        Throws.ThrowIfIndexGreaterThanOrEqual(offset, length);
        return offset;
    }

    /// <summary>
    /// Get offset of start and end index of <paramref name="range"/>, no overflow check
    /// </summary>
    public static (int Start, int End) GetStartAndEndOffset(this Range range, int length)
    {
        return (range.Start.GetOffset(length), range.End.GetOffset(length));
    }

    public static (int Start, int End) GetCheckedStartAndEndOffset(this Range range, int length)
    {
        var (start, end) = range.GetStartAndEndOffset(length);
        Throws.ThrowIfGreaterThan((uint)end, (uint)length);
        Throws.ThrowIfGreaterThan((uint)start, (uint)end);
        return (start, end);
    }

    public static void ValidateSliceArgs(int start, int sliceLength, int count)
    {
        Throws.ThrowIfNegative(start);
        Throws.ThrowIfNegative(sliceLength);
        Throws.ThrowIfGreaterThan(start + sliceLength, count);
    }
}
