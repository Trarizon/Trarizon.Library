namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraAlgorithm
{
    internal static class Utils
    {
        public static void Swap<T>(ref T left, ref T right) => (left, right) = (right, left);

        public static int Cmp<T>(T left, T right, IComparer<T> comparer) => comparer.Compare(left, right);

        public static int CmpReverse<T>(T left, T right, IComparer<T> comparer) => -comparer.Compare(left, right);
    }
}
