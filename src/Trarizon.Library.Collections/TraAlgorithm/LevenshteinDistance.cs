using System.Buffers;
using System.Diagnostics;
using Trarizon.Library.Collections.Buffers;
using Trarizon.Library.Collections.Generic;

namespace Trarizon.Library.Collections;
public static partial class TraAlgorithm
{
    public static int LevenshteinDistance(string from, string to) => LevenshteinDistance(from.AsSpan(), to.AsSpan());

    public static int LevenshteinDistance<T>(ReadOnlySpan<T> from, ReadOnlySpan<T> to)
    {
        if (from.IsEmpty)
            return to.Length;
        if (to.IsEmpty)
            return from.Length;

        //   f r o m
        // t
        // o
        using var d_ = ArrayPool<int>.Shared.RentAsSpan(from.Length * to.Length, out var buffer);

#if DEBUG
        foreach (ref var b in buffer) {
            b = -1;
        }
#endif
        if (typeof(T).IsValueType) {
            LevenshteinDistanceImplDefault(buffer, from, to);
        }
        else {
            LevenshteinDistanceImplComparer(buffer, from, to, EqualityComparer<T>.Default);
        }

        Debug.Assert(buffer.ToArray().All(i => i != -1));
        return buffer[^1];
    }

    public static int LevenshteinDistance<T, TComparer>(ReadOnlySpan<T> from, ReadOnlySpan<T> to, TComparer comparer) where TComparer : IEqualityComparer<T>
    {
        if (from.IsEmpty)
            return to.Length;
        if (to.IsEmpty)
            return from.Length;

        using var d_ = ArrayPool<int>.Shared.RentAsSpan(from.Length * to.Length, out var buffer);

        LevenshteinDistanceImplComparer(buffer, from, to, comparer);

        return buffer[^1];
    }

    private static void LevenshteinDistanceImplDefault<T>(Span<int> buffer, ReadOnlySpan<T> from, ReadOnlySpan<T> to)
    {
        Debug.Assert(buffer.Length >= from.Length * to.Length);
        var initDelta = EqualityComparer<T>.Default.Equals(from[0], to[0]) ? 0 : 1;
        for (int i = 0; i < from.Length; i++) {
            buffer[i] = i + initDelta;
        }
        for (int i = 1; i < to.Length; i++) {
            buffer[i * from.Length] = i + initDelta;
        }

        for (int col = 1; col < to.Length; col++) {
            for (int row = 1; row < from.Length; row++) {
                var up = buffer[(col - 1) * from.Length + row] + 1;
                var lf = buffer[col * from.Length + row - 1] + 1;
                var lu = buffer[(col - 1) * from.Length + row - 1];
                if (EqualityComparer<T>.Default.Equals(from[row], to[col]))
                    lu++;
                var cur = Math.Min(up, Math.Min(lf, lu));
                buffer[col * from.Length + row] = cur;
            }
        }
    }

    private static void LevenshteinDistanceImplComparer<T, TComparer>(Span<int> buffer, ReadOnlySpan<T> from, ReadOnlySpan<T> to, TComparer comparer) where TComparer : IEqualityComparer<T>
    {
        Debug.Assert(buffer.Length >= from.Length * to.Length);
        var initDelta = comparer.Equals(from[0], to[0]) ? 0 : 1;
        for (int i = 0; i < from.Length; i++) {
            buffer[i] = i + initDelta;
        }
        for (int i = 1; i < to.Length; i++) {
            buffer[i * from.Length] = i + initDelta;
        }

        for (int col = 1; col < to.Length; col++) {
            for (int row = 1; row < from.Length; row++) {
                var up = buffer[(col - 1) * from.Length + row] + 1;
                var lf = buffer[col * from.Length + row - 1] + 1;
                var lu = buffer[(col - 1) * from.Length + row - 1];
                if (comparer.Equals(from[row], to[col]))
                    lu++;
                var cur = Math.Min(up, Math.Min(lf, lu));
                buffer[col * from.Length + row] = cur;
            }
        }
    }

    private static void LevenshteinDistanceRouteImplDefault<T>(Span<(int Distance, LevenshteinRouteEditKind)> buffer, ReadOnlySpan<T> from, ReadOnlySpan<T> to)
    {
        Debug.Assert(buffer.Length >= from.Length * to.Length);
        var initDelta = EqualityComparer<T>.Default.Equals(from[0], to[0]) ? 0 : 1;
        for (int i = 0; i < from.Length; i++) {
            buffer[i] = (i + initDelta, LevenshteinRouteEditKind.Delete);
        }
        for (int i = 1; i < to.Length; i++) {
            buffer[i * from.Length] = (i + initDelta, LevenshteinRouteEditKind.Add);
        }

        for (int col = 1; col < to.Length; col++) {
            for (int row = 1; row < from.Length; row++) {
                var up = buffer[(col - 1) * from.Length + row].Distance + 1;
                var lf = buffer[col * from.Length + row - 1].Distance + 1;
                var lu = buffer[(col - 1) * from.Length + row - 1].Distance;
                bool edit = EqualityComparer<T>.Default.Equals(from[row], to[col]);
                if (edit) lu++;
                int cur;
                LevenshteinRouteEditKind kind;
                if (up > lf) {
                    if (lf > lu)
                        (cur, kind) = (lu, edit ? LevenshteinRouteEditKind.Edit : LevenshteinRouteEditKind.None);
                    else
                        (cur, kind) = (lf, LevenshteinRouteEditKind.Delete);
                }
                else {
                    if (up > lu)
                        (cur, kind) = (lu, edit ? LevenshteinRouteEditKind.Edit : LevenshteinRouteEditKind.None);
                    else
                        (cur, kind) = (lf, LevenshteinRouteEditKind.Add);
                }
                buffer[col * from.Length + row] = (cur, kind);
            }
        }
    }

    public enum LevenshteinRouteEditKind { None, Add, Delete, Edit }
}
