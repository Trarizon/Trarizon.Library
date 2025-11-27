using System.Numerics;

namespace Trarizon.Library.Collections;

public static partial class TraAlgorithm
{
    internal static class Utils
    {
        public static void Swap<T>(ref T left, ref T right) => (left, right) = (right, left);
    }

    private interface ISorter<T>
    {
        bool IsInOrder(T left, T right);
        bool IsInOrderOrEqual(T left, T right);
    }

#if NET7_0_OR_GREATER

    private readonly struct ComparisonOperatorsAscSorter<T> : ISorter<T> where T : IComparisonOperators<T, T, bool>
    {
        public bool IsInOrder(T left, T right) => left < right;
        public bool IsInOrderOrEqual(T left, T right) => left <= right;
    }

    private readonly struct ComparisonOperatorsDescSorter<T> : ISorter<T> where T : IComparisonOperators<T, T, bool>
    {
        public bool IsInOrder(T left, T right) => left > right;
        public bool IsInOrderOrEqual(T left, T right) => left >= right;
    }

#endif

    private readonly struct ComparerAscSorter<T, TComparer>(TComparer comparer) : ISorter<T> where TComparer : IComparer<T>
    {
        public bool IsInOrder(T left, T right) => comparer.Compare(left, right) < 0;
        public bool IsInOrderOrEqual(T left, T right) => comparer.Compare(left, right) <= 0;
    }
}
