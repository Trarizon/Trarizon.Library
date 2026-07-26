namespace Trarizon.Library.Collections;

#if NETSTANDARD

internal static partial class Polyfills
{
    extension(Array)
    {
        public static int MaxLength => 0X7FFFFFC7;
    }

#if NETSTANDARD2_0

    public static Span<T> AsSpan<T>(this T[] array, Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(array.Length);
        return array.AsSpan(ofs, len);
    }

#endif
}

#endif
