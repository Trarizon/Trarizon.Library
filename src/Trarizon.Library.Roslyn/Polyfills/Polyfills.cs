using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Roslyn;

internal static class Polyfills
{
#if !IMMUTABLE_MARSHAL

    public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlySpan<T> span)
    {
        var builder = ImmutableArray.CreateBuilder<T>(span.Length);
        foreach (var item in span)
            builder.Add(item);
        return builder.MoveToImmutable();
    }

#endif

    extension(string)
    {
        public static string Join<T>(string separator, ReadOnlySpan<T> values)
        {
            if (values.IsEmpty)
                return string.Empty;
            var sb = new DefaultInterpolatedStringHandler((values.Length - 1) * separator.Length, values.Length);
            sb.AppendFormatted(values[0]);
            foreach (var item in values[1..])
            {
                sb.AppendLiteral(separator);
                sb.AppendFormatted(item);
            }
            return sb.ToStringAndClear();
        }
    }
}
