#if NETSTANDARD

using Trarizon.Library.Numerics;

namespace Trarizon.Library.Text;
public static partial class TraString
{
    public static ReadOnlySpan<char> AsSpan(this string str, Range range)
    {
        var (ofs, len) = range.GetCheckedOffsetAndLength(str.Length);
        return str.AsSpan(ofs, len);
    }
}

#endif
