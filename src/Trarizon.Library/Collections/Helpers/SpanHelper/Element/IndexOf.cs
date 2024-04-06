namespace Trarizon.Library.Collections.Helpers;
partial class SpanHelper
{
    public static int IndexOf<T>(this Span<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;

    public static int IndexOf<T>(this ReadOnlySpan<T> span, T value, int startIndex) where T : IEquatable<T>?
        => span[startIndex..].IndexOf(value) + startIndex;
}
