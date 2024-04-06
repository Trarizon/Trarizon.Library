using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
partial class SpanHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAt<T>(this Span<T> span, int index, [MaybeNullWhen(false)] out T value)
        => span.AsReadOnly().TryAt(index, out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAt<T>(this ReadOnlySpan<T> span, int index, [MaybeNullWhen(false)] out T value)
    {
        if (index < span.Length && index >= 0) {
            value = span[index];
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAt<T>(this Span<T> span, Index index, [MaybeNullWhen(false)] out T value)
        => span.AsReadOnly().TryAt(index, out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAt<T>(this ReadOnlySpan<T> span, Index index, [MaybeNullWhen(false)] out T value)
    {
        var i = index.Value;
        if (i < span.Length) {
            value = span[i];
            return true;
        }
        value = default;
        return false;
    }
}
