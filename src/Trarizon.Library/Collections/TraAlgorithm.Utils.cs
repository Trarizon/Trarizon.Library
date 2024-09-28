using System.Numerics;

namespace Trarizon.Library.Collections;
partial class TraAlgorithm
{
    internal static class Utils
    {
        public static void Swap<T>(ref T left, ref T right) => (left, right) = (right, left);

        public static bool AscOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left < right;

        public static bool AscEqOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left <= right;

        public static bool DescOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left > right;

        public static bool DescEqOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left >= right;

        public static int Cmp<T>(T left, T right, IComparer<T> comparer) => comparer.Compare(left, right);

        public static int CmpReverse<T>(T left, T right, IComparer<T> comparer) => -comparer.Compare(left, right);
    }
}
