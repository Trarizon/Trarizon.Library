using System.Diagnostics;

namespace Trarizon.Library.Algorithms;

public static class CollectionAlgorithm
{
    public static T NthSmallest<T>(T[] arr, int n, IComparer<T>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(n, arr.Length);

        if (arr.Length == 1)
            return arr[0];

        comparer ??= Comparer<T>.Default;

        var left = 0;
        var right = arr.Length - 1;
        var val = arr[0];

        while (left < right) {
            while (left < right) {
                if (comparer.Compare(arr[right], val) >= 0) {
                    right--;
                    continue;
                }
                arr[left] = arr[right];
                left++;
                break;
            }

            while (left < right) {
                if (comparer.Compare(arr[left], val) <= 0) {
                    left++;
                    continue;
                }
                arr[right] = arr[left];
                right--;
                break;
            }
        }

        Debug.Assert(left == right);
        if (n < left) {
            return NthSmallest(arr[0..left], n, comparer);
        }
        else if (n == left) {
            return val;
        }
        else {
            return NthSmallest(arr[(left + 1)..], n - left - 1, comparer);
        }
    }

    public static void QSort(Span<int> arr, IComparer<int>? comparer = null)
    {
        if (arr.Length <= 1)
            return;

        comparer ??= Comparer<int>.Default;

        var left = 0;
        var right = arr.Length - 1;
        var val = arr[0];

        while (left < right) {
            while (left < right) {
                if (comparer.Compare(arr[right], val) >= 0) {
                    right--;
                    continue;
                }
                arr[left] = arr[right];
                left++;
                break;
            }

            while (left < right) {
                if (comparer.Compare(arr[left], val) <= 0) {
                    left++;
                    continue;
                }
                arr[right] = arr[left];
                right--;
                break;
            }
        }

        Debug.Assert(left == right);
        arr[left] = val;
        QSort(arr[0..left], comparer);
        QSort(arr[(left + 1)..], comparer);
    }
}
