using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Common;
public static class TraIndex
{
    /// <summary>
    /// <see cref="Index.GetOffset(int)"/>, and check if the offset is in [0, <paramref name="length"/>),
    /// throw if out of range
    /// </summary>
    public static int GetCheckedOffset(this Index index, int length)
    {
        var offset = index.GetOffset(length);
        Guard.IsLessThan((uint)offset, (uint)length);
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
        Guard.IsLessThanOrEqualTo((uint)end, (uint)length);
        Guard.IsLessThanOrEqualTo((uint)start, (uint)end);
        return (start, end);
    }

    public static void ValidateSliceArgs(int start, int sliceLength, int count)
    {
        Guard.IsGreaterThanOrEqualTo(start, 0);
        Guard.IsGreaterThanOrEqualTo(sliceLength, 0);
        Guard.IsLessThanOrEqualTo(start + sliceLength, count);
    }
}
