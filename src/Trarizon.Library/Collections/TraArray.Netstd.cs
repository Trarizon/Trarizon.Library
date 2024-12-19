#if NETSTANDARD2_0

using Trarizon.Library.Numerics;

namespace Trarizon.Library.Collections;
partial class TraArray
{
    public static int MaxLength => 0X7FFFFFC7;

    public static Span<T> AsSpan<T>(this T[] array, Range range)
    {
        var (ofs, len) = range.GetCheckedOffsetAndLength(array.Length);
        return array.AsSpan(ofs, len);
    }
}

#endif
