using System.Numerics;

namespace Trarizon.Library.Collections.Helpers;
partial class SpanQuery
{
    public static T Sum<T>(this ReadOnlySpan<T> values) where T : INumber<T>
    {
        T res = T.Zero;
        foreach (var item in values)
            res += item;
        return res;
    }

    public static T Sum<T>(this Span<T> values) where T : INumber<T>
    {
        T res = T.Zero;
        foreach (var item in values)
            res += item;
        return res;
    }
}
