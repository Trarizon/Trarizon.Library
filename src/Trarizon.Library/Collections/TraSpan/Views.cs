using CommunityToolkit.HighPerformance;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
partial class TraSpan
{
#if !NETSTANDARD2_0
    
    public static ReversedSpan<T> AsReversed<T>(this Span<T> span)
        => new(ref span.DangerousGetReferenceAt(span.Length - 1), span.Length);

    public static ReadOnlyReversedSpan<T> AsReversed<T>(this ReadOnlySpan<T> span)
        => new(in span.DangerousGetReferenceAt(span.Length - 1), span.Length);

#endif
}
