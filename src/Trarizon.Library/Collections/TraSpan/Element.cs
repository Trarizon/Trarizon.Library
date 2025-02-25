using System.Diagnostics.CodeAnalysis;

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

    public static T? ElementAtOfDefault<T>(this Span<T> span, int index)
        => ElementAtOrDefault((ReadOnlySpan<T>)span, index);
}