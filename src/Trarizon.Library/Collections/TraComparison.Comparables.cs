#if NETSTANDARD
#pragma warning disable CS8604 // 引用类型参数可能为 null。
#endif

using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraComparison
{
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
