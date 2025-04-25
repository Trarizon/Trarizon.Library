using CommunityToolkit.HighPerformance;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static ReversedSpan<T> AsReversed<T>(this Span<T> span)
#if NETSTANDARD
        => new(span);
#else
        => new(ref span.DangerousGetReferenceAt(span.Length - 1), span.Length);
#endif

    public static ReadOnlyReversedSpan<T> AsReversed<T>(this ReadOnlySpan<T> span)
#if NETSTANDARD
        => new(span);
#else
        => new(in span.DangerousGetReferenceAt(span.Length - 1), span.Length);
#endif
}
