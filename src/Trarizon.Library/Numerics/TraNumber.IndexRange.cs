using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Numerics;
public static partial class TraNumber
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
    /// <see cref="Range.GetOffsetAndLength(int)"/>, and check if the offset and count is in [0, <paramref name="length"/>),
    /// throw if out of range
    /// </summary>
    public static (int Offset, int Length) GetCheckedOffsetAndLength(this Range range, int length)
    {
        var (ofs, len) = range.GetOffsetAndLength(length);
        Guard.IsGreaterThanOrEqualTo(ofs, 0);
        Guard.IsGreaterThanOrEqualTo(len, 0);
        Guard.IsLessThan(ofs + len, length);
        return (ofs, len);
    }

    public static bool CheckSliceArgs(int start, int sliceLength, int count)
    {
        Guard.IsGreaterThanOrEqualTo(start, 0);
        Guard.IsGreaterThanOrEqualTo(sliceLength, 0);
        Guard.IsLessThanOrEqualTo(start + sliceLength, count);
        return true;
    }
}
