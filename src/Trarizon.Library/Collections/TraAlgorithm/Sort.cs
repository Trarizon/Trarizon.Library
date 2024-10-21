﻿using CommunityToolkit.HighPerformance;
using System.Numerics;

namespace Trarizon.Library.Collections;
partial class TraAlgorithm
{
    public static unsafe void BubbleSort<T>(Span<T> values, bool descending = false) where T : IComparisonOperators<T, T, bool>
    {
        if (values.Length <= 1)
            return;

        bool inOrder;
        delegate*<T, T, bool> cmp = descending ? &Utils.AscOp : &Utils.DescOp;

        for (int i = 0; i < values.Length; i++) {
            inOrder = true;
            for (int j = values.Length - 1; j > i; j--) {
                ref var left = ref values.DangerousGetReferenceAt(j - 1);
                ref var right = ref values.DangerousGetReferenceAt(j);
                if (cmp(left, right)) {
                    Utils.Swap(ref left, ref right);
                    inOrder = false;
                }
            }
            if (inOrder)
                return;
        }
    }

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
                ref var left = ref values.DangerousGetReferenceAt(j - 1);
                ref var right = ref values.DangerousGetReferenceAt(j);
                if (comparer.Compare(left, right) > 0) {
                    Utils.Swap(ref left, ref right);
                    inOrder = false;
                }
            }
            if (inOrder)
                return;
        }
    }

    public static unsafe void InsertionSort<T>(Span<T> values, bool descending = false) where T : IComparisonOperators<T, T, bool>
    {
        if (values.Length <= 1)
            return;

        delegate*<T, T, bool> cmp = descending ? &Utils.DescEqOp : &Utils.AscEqOp;

        for (int i = 1; i < values.Length; i++) {
            int j = i - 1;
            for (; j >= 0; j--) {
                var left = values.DangerousGetReferenceAt(j);
                var right = values.DangerousGetReferenceAt(i);
                if (cmp(left, right)) {
                    break;
                }
            }
            values.MoveTo(i, j + 1);
        }
    }

    public static void InsertionSort<T>(Span<T> values, IComparer<T>? comparer)
        => InsertionSort<T, IComparer<T>>(values, comparer ?? Comparer<T>.Default);

    private static void InsertionSort<T, TComparer>(Span<T> values, TComparer comparer) where TComparer : IComparer<T>
    {
        if (values.Length <= 1)
            return;

        for (int i = 1; i < values.Length; i++) {
            int j = i - 1;
            for (; j >= 0; j--) {
                var left = values.DangerousGetReferenceAt(j);
                var right = values.DangerousGetReferenceAt(i);
                if (comparer.Compare(left, right) <= 0) {
                    break;
                }
            }
            values.MoveTo(i, j + 1);
        }
    }
}