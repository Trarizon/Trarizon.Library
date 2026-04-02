using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;

internal static class Utility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReferenceAt<T>(ReadOnlySpan<T> span, int index)
       => ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index);

    // This may conflict to CommunityToolkit.HighPerformance, so we don't public it
    // But we public a non-extension method for .NET Standard
    /// <remarks>
    /// As CollectionsMarshal doesnt exists on .NET Standard 2.0, this use a very tricky way
    /// to get the underlying array. Actually I'm not sure if this works correctly in all runtime...
    /// (at least it works on Unity
    /// </remarks>
    internal static Span<T> AsSpan<T>(this List<T> list)
    {
#if NETSTANDARD
        return TraCollection.AsSpan(list);
#else
        return CollectionsMarshal.AsSpan(list);
#endif
    }

    internal static bool TryGetSpan<T>(this IEnumerable<T> source, out ReadOnlySpan<T> span)
    {
        if (source.GetType() == typeof(T[])) {
            span = Unsafe.As<T[]>(source).AsSpan();
            return true;
        }
        if (source.GetType() == typeof(List<T>)) {
            var list = Unsafe.As<List<T>>(source);
            span = list.AsSpan();
            return true;
        }
        span = default;
        return false;
    }
}
