using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
partial class TraSpan
{
    public static ReversedSpan<T> AsReversed<T>(this Span<T> span) => new(span);

    public static ReadOnlyReversedSpan<T> AsReversed<T>(this ReadOnlySpan<T> span) => new(span);
}
