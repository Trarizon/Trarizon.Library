using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections.Helpers;
partial class SpanHelper
{
    public static ReversedSpan<T> ToReversedSpan<T>(this Span<T> span) => new(span);

    public static ReversedReadOnlySpan<T> ToReversedSpan<T>(this ReadOnlySpan<T> span) => new(span);
}
