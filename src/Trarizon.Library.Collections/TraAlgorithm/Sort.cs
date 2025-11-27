using System.Numerics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;

public static partial class TraAlgorithm
{
#if NET7_0_OR_GREATER

    [OverloadResolutionPriority(1)]
    public static void BubbleSort<T>(Span<T> values, bool descending = false) where T : IComparisonOperators<T, T, bool>
    {
        if (descending)
            BubbleSortImpl(values, new ComparisonOperatorsDescSorter<T>());
        else
            BubbleSortImpl(values, new ComparisonOperatorsAscSorter<T>());
    }

#endif

    public static void BubbleSort<T>(Span<T> values, IComparer<T>? comparer = null)
        => BubbleSort<T, IComparer<T>>(values, comparer ?? Comparer<T>.Default);

    public static void BubbleSort<T, TComparer>(Span<T> values, TComparer comparer) where TComparer : IComparer<T>
        => BubbleSortImpl(values, new ComparerAscSorter<T, TComparer>(comparer));

    private static void BubbleSortImpl<T, TSorter>(Span<T> values, TSorter sorter) where TSorter : ISorter<T>
    {
        if (values.Length <= 1)
            return;

        bool inOrder;

        for (int i = 0; i < values.Length; i++) {
            inOrder = true;
            for (int j = values.Length - 1; j > i; j--) {
                ref var left = ref values[j - 1];
                ref var right = ref values[j];
                if (!sorter.IsInOrder(left, right)) {
                    Utils.Swap(ref left, ref right);
                    inOrder = false;
                }
            }
            if (inOrder)
                return;
        }
    }

#if NET7_0_OR_GREATER

    [OverloadResolutionPriority(1)]
    public static unsafe void InsertionSort<T>(Span<T> values, bool descending = false) where T : IComparisonOperators<T, T, bool>
    {
        if (descending)
            InsertionSortImpl(values, new ComparisonOperatorsDescSorter<T>());
        else
            InsertionSortImpl(values, new ComparisonOperatorsAscSorter<T>());
    }

#endif

    public static void InsertionSort<T>(Span<T> values, IComparer<T>? comparer=null)
        => InsertionSort<T, IComparer<T>>(values, comparer ?? Comparer<T>.Default);

    private static void InsertionSort<T, TComparer>(Span<T> values, TComparer comparer) where TComparer : IComparer<T>
        => InsertionSortImpl(values, new ComparerAscSorter<T, TComparer>(comparer));

    private static void InsertionSortImpl<T, TSorter>(Span<T> values, TSorter sorter) where TSorter : ISorter<T>
    {
        if (values.Length <= 1)
            return;

        for (int i = 1; i < values.Length; i++) {
            int j = i - 1;
            for (; j >= 0; j--) {
                var left = values[j];
                var right = values[i];
                if (sorter.IsInOrderOrEqual(left, right)) {
                    break;
                }
            }
            values.MoveTo(i, j + 1);
        }
    }
}
