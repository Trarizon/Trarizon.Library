using System.Numerics;

namespace Trarizon.Library.Collections;
partial class TraAlgorithm
{
    internal static class Utils
    {
        public static void Swap<T>(ref T left, ref T right) => (left, right) = (right, left);

#if NET7_0_OR_GREATER
      
        public static bool AscOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left < right;

        public static bool AscEqOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left <= right;

        public static bool DescOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left > right;

        public static bool DescEqOp<T>(T left, T right) where T : IComparisonOperators<T, T, bool> => left >= right;
    
#endif
    }
}
