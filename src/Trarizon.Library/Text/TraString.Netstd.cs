namespace Trarizon.Library.Text;
public static partial class TraString
{
#if NETSTANDARD

    public static ReadOnlySpan<char> AsSpan(this string str, Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(str.Length);
        return str.AsSpan(ofs, len);
    }

#endif
}
