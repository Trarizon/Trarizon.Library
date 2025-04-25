namespace Trarizon.Library.Collections;
public static partial class TraArray
{
#if NETSTANDARD

    public static int MaxLength => 0X7FFFFFC7;

#endif

#if NETSTANDARD2_0
    
    public static Span<T> AsSpan<T>(this T[] array, Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(array.Length);
        return array.AsSpan(ofs, len);
    }

#endif
}

