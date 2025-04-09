using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
internal static partial class ArrayGrowHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeManaged<T>(Span<T> span,int index)
    {
#if !NETSTANDARD2_0
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif
        span[index] = default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeManaged<T>(Span<T> span)
    {
#if !NETSTANDARD2_0
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif
        span.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeManaged<T>(T[] array, Range range)
    {
#if !NETSTANDARD2_0
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif
        array.AsSpan(range).Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeManaged<T>(T[] array, int index, int length)
    {
#if !NETSTANDARD2_0
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif
        array.AsSpan(index, length).Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ShiftLeftForRemove<T>(T[] array, int originalTruncationLength, int removeStart, int removeCount)
    {
        var copyStart = removeStart + removeCount;
        var length = originalTruncationLength - copyStart;
        array.AsSpan(copyStart, length).CopyTo(array.AsSpan(removeStart, length));
        FreeManaged(array, removeStart + length, removeCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ShiftLeftForRemoveNonFree<T>(T[] array, int originalTruncationLength, int removeStart, int removeCount)
    {
        var copyStart = removeStart + removeCount;
        var length = originalTruncationLength - copyStart;
        array.AsSpan(copyStart, length).CopyTo(array.AsSpan(removeStart, length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ShiftRightForInsert<T>(T[] array, int originalTruncationLength, int insertStartIndex, int insertLength)
    {
        Debug.Assert(array.Length >= originalTruncationLength + insertLength);
        Debug.Assert(insertStartIndex <= originalTruncationLength);

        var length = originalTruncationLength - insertStartIndex;
        array.AsSpan(insertStartIndex, length).CopyTo(array.AsSpan(insertStartIndex + insertLength, length));
    }

    public static void Grow<T>(ref T[] array, int expectedLength, int copyLength)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(copyLength <= array.Length);

        var originalArray = array;
        GrowNonMove(ref array, expectedLength);
        originalArray.AsSpan(..copyLength).CopyTo(array);
    }

    public static void GrowNonMove<T>(ref T[] array, int expectedLength)
    {
#if NETSTANDARD
        GrowNonMove(ref array, expectedLength, TraArray.MaxLength);
#else
        GrowNonMove(ref array, expectedLength, Array.MaxLength);
#endif
    }

    public static void GrowNonMove<T>(ref T[] array, int expectedLength, int maxLength)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(expectedLength <= maxLength);
        array = new T[GetNewLength(array.Length, expectedLength, maxLength)];
    }

    public static void GrowForInsertion<T>(ref T[] array, int expectedLength, int copyCount, int insertIndex, int insertCount)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(insertIndex <= array.Length);
        Debug.Assert(insertIndex >= 0);
        Debug.Assert(insertCount >= 0);

        var originalArray = array;
        GrowNonMove(ref array, expectedLength);
        if (insertIndex > 0) {
            originalArray.AsSpan(..insertIndex).CopyTo(array);
        }
        if (insertIndex < copyCount) {
            var count = copyCount - insertIndex;
            originalArray.AsSpan(insertIndex, count).CopyTo(array.AsSpan(insertIndex + insertCount, count));
        }
    }

    private static int GetNewLength(int length, int expectedLength, int maxLength)
    {
        Debug.Assert(length < expectedLength);

        int newLen;
        if (length == 0)
            newLen = Math.Min(4, maxLength);
        else
            newLen = Math.Min(length * 2, maxLength);

        if (newLen < expectedLength)
            newLen = expectedLength;
        return newLen;
    }
}
