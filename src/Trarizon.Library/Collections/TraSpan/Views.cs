using CommunityToolkit.HighPerformance;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
#if !NETSTANDARD
    
    public static ReversedSpan<T> AsReversed<T>(this Span<T> span)
        => new(ref span.DangerousGetReferenceAt(span.Length - 1), span.Length);

    public static ReadOnlyReversedSpan<T> AsReversed<T>(this ReadOnlySpan<T> span)
        => new(in span.DangerousGetReferenceAt(span.Length - 1), span.Length);

#endif
}
