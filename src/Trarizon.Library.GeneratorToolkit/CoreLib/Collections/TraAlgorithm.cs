using CommunityToolkit.HighPerformance;
using System.Numerics;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
public static partial class TraAlgorithm
{
    public static unsafe void BubbleSort<T>(Span<T> values, IComparer<T>? comparer = null, bool descending = false)
    {
        if (values.Length <= 1)
            return;

        bool inOrder;
        comparer ??= Comparer<T>.Default;
        delegate*<T, T, IComparer<T>, int> cmp = descending ? &Utils.CmpReverse : &Utils.Cmp;

        for (int i = 0; i < values.Length; i++) {
            inOrder = true;
            for (int j = values.Length - 1; j > i; j--) {
                ref var left = ref values.DangerousGetReferenceAt(j - 1);
                ref var right = ref values.DangerousGetReferenceAt(j);
                if (cmp(left, right, comparer) > 0) {
                    Utils.Swap(ref left, ref right);
                    inOrder = false;
                }
            }
            if (inOrder)
                return;
        }
    }

    public static unsafe void InsertionSort<T>(Span<T> values, IComparer<T>? comparer = null, bool descending = false)
    {
        if (values.Length <= 1)
            return;

        comparer ??= Comparer<T>.Default;
        delegate*<T, T, IComparer<T>, int> cmp = descending ? &Utils.CmpReverse : &Utils.Cmp;

        for (int i = 1; i < values.Length; i++) {
            int j = i - 1;
            for (; j >= 0; j--) {
                var left = values.DangerousGetReferenceAt(j);
                var right = values.DangerousGetReferenceAt(i);
                if (cmp(left, right, comparer) <= 0) {
                    break;
                }
            }
            values.MoveTo(i, j + 1);
        }
    }
}
