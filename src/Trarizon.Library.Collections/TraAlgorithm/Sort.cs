using System.Numerics;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;
public static partial class TraAlgorithm
{
#if NET7_0_OR_GREATER

    public static unsafe void BubbleSort<T>(Span<T> values, bool descending = false) where T : IComparisonOperators<T, T, bool>
    {
        if (values.Length <= 1)
            return;

        bool inOrder;
        delegate*<T, T, bool> cmp = descending ? &Utils.AscOp : &Utils.DescOp;

        for (int i = 0; i < values.Length; i++) {
            inOrder = true;
            for (int j = values.Length - 1; j > i; j--) {
                ref var left = ref Unsafes.GetReferenceAt(values, j - 1);
                ref var right = ref Unsafes.GetReferenceAt(values, j);
                if (cmp(left, right)) {
                    Utils.Swap(ref left, ref right);
                    inOrder = false;
                }
            }
            if (inOrder)
                return;
        }
    }

#endif

    public static void BubbleSort<T>(Span<T> values, IComparer<T>? comparer)
        => BubbleSort<T, IComparer<T>>(values, comparer ?? Comparer<T>.Default);

    public static void BubbleSort<T, TComparer>(Span<T> values, TComparer comparer) where TComparer : IComparer<T>
    {
        if (values.Length <= 1)
            return;

        bool inOrder;

        for (int i = 0; i < values.Length; i++) {
            inOrder = true;
            for (int j = values.Length - 1; j > i; j--) {
                ref var left = ref Unsafes.GetReferenceAt(values, j - 1);
                ref var right = ref Unsafes.GetReferenceAt(values, j);
                if (comparer.Compare(left, right) > 0) {
                    Utils.Swap(ref left, ref right);
                    inOrder = false;
                }
            }
            if (inOrder)
                return;
        }
    }

#if NET7_0_OR_GREATER

    public static unsafe void InsertionSort<T>(Span<T> values, bool descending = false) where T : IComparisonOperators<T, T, bool>
    {
        if (values.Length <= 1)
            return;

        delegate*<T, T, bool> cmp = descending ? &Utils.DescEqOp : &Utils.AscEqOp;

        for (int i = 1; i < values.Length; i++) {
            int j = i - 1;
            for (; j >= 0; j--) {
                var left = Unsafes.GetReferenceAt(values, j);
                var right = Unsafes.GetReferenceAt(values, i);
                if (cmp(left, right)) {
                    break;
                }
            }
            values.MoveTo(i, j + 1);
        }
    }

#endif

    public static void InsertionSort<T>(Span<T> values, IComparer<T>? comparer)
        => InsertionSort<T, IComparer<T>>(values, comparer ?? Comparer<T>.Default);

    private static void InsertionSort<T, TComparer>(Span<T> values, TComparer comparer) where TComparer : IComparer<T>
    {
        if (values.Length <= 1)
            return;

        for (int i = 1; i < values.Length; i++) {
            int j = i - 1;
            for (; j >= 0; j--) {
                var left = Unsafes.GetReferenceAt(values, j);
                var right = Unsafes.GetReferenceAt(values, i);
                if (comparer.Compare(left, right) <= 0) {
                    break;
                }
            }
            values.MoveTo(i, j + 1);
        }
    }
}
