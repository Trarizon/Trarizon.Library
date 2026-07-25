namespace Trarizon.Library;

#if NETSTANDARD

internal static partial class Polyfills
{
    public static ReadOnlySpan<char> AsSpan(this string str, Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(str.Length);
        return str.AsSpan(ofs, len);
    }
}

#endif
