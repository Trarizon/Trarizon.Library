using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
partial class SpanHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span) => span;
}
