﻿#if NETSTANDARD2_0

using Trarizon.Library.Numerics;

namespace Trarizon.Library.Text;
partial class TraString
{
    public static ReadOnlySpan<char> AsSpan(this string str, Range range)
    {
        var (ofs, len) = range.GetCheckedOffsetAndLength(str.Length);
        return str.AsSpan(ofs, len);
    }
}

#endif