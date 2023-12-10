using System;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SpanExtensions
{
    public static unsafe string CreateString(this ReadOnlySpan<char> charSpan)
    {
        fixed (char* ptr = charSpan) {
            return new(ptr, 0, charSpan.Length);
        }
    }

    public static string CreateString(this Span<char> charSpan)
        => CreateString((ReadOnlySpan<char>)charSpan);
}
