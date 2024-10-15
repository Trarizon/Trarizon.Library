using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static class TraComparison
{
    public static ReversedComparer<T, IComparer<T>> Reverse<T>(this IComparer<T> comparer) => new(comparer);


    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct ReversedComparer<T, TComparer>(TComparer comparer) : IComparer<T> where TComparer : IComparer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T? x, T? y) => comparer.Compare(y, x);
    }

    internal readonly struct ComparerComparable<T, TComparer>(T value, TComparer comparer) : IComparable<T> where TComparer : IComparer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(T? other) => comparer.Compare(value, other);
    }

    /// <summary>
    /// A wrapper, if value == other, returns as value &lt; other, never return 0
    /// </summary>
    internal readonly struct GreaterOrNotComparable<T, TComparable>(TComparable value) : IComparable<T> where TComparable : IComparable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(T? other)
        {
            var res = value.CompareTo(other);
            if (res == 0)
                return -1;
            return res;
        }
    }

    /// <summary>
    /// A wrapper, if value == other, returns as value > other, never return 0
    /// </summary>
    internal readonly struct LessOrNotComparable<T, TComparable>(TComparable value) : IComparable<T> where TComparable : IComparable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(T? other)
        {
            var res = value.CompareTo(other);
            if (res == 0)
                return 1;
            return res;
        }
    }
}
