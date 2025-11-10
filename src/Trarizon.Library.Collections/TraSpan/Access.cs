using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static bool TryAt<T>(this ReadOnlySpan<T> span, int index, [MaybeNullWhen(false)] out T item)
    {
        if ((uint)index < span.Length) {
            item = span[index];
            return true;
        }
        item = default;
        return false;
    }

    public static bool TryAt<T>(this Span<T> span, int index, [MaybeNullWhen(false)] out T item)
        => TryAt((ReadOnlySpan<T>)span, index, out item);

    public static T? ElementAtOrDefault<T>(this ReadOnlySpan<T> span, int index)
    {
        if ((uint)index < span.Length) {
            return span[index];
        }
        return default;
    }

    public static T? ElementAtOrDefault<T>(this Span<T> span, int index)
        => ElementAtOrDefault((ReadOnlySpan<T>)span, index);

    public static ReversedSpan<T> AsReversed<T>(this Span<T> span)
#if NETSTANDARD
        => new(span);
#else
        => new(ref Utility.GetReferenceAt(span, span.Length - 1), span.Length);
#endif

    public static ReadOnlyReversedSpan<T> AsReversed<T>(this ReadOnlySpan<T> span)
#if NETSTANDARD
        => new(span);
#else
        => new(in Utility.GetReferenceAt(span, span.Length - 1), span.Length);
#endif
}
