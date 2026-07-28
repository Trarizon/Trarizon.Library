using Trarizon.Library.CompilerServices;

namespace Trarizon.Library.Memory;

public static partial class TraSpan
{
    public static ReversedSpan<T> AsReversed<T>(this Span<T> span)
#if NETSTANDARD
        => new(span);
#else
        => new(ref Internal.GetReferenceAt(span, span.Length - 1), span.Length);
#endif

    public static ReadOnlyReversedSpan<T> AsReversed<T>(this ReadOnlySpan<T> span)
#if NETSTANDARD
        => new(span);
#else
        => new(in Internal.GetReferenceAt(span, span.Length - 1), span.Length);
#endif
}