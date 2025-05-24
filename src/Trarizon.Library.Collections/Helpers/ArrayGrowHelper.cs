using System.Diagnostics;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_0
using RuntimeHelpers = Trarizon.Library.Collections.Helpers.PfRuntimeHelpers;
#endif
#if NETSTANDARD
using ArrayMaxLengthProvider = Trarizon.Library.Collections.TraArray;
#else
using ArrayMaxLengthProvider = System.Array;
#endif

namespace Trarizon.Library.Collections.Helpers;
internal static partial class ArrayGrowHelper
{
    #region FreeIfReferenceOrContainsReferences

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeIfReferenceOrContainsReferences<T>(Span<T> span, int index)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            span[index] = default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeIfReferenceOrContainsReferences<T>(Span<T> span)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            span.Clear();
    }

    #endregion

    #region ShiftLeft/Right

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ShiftLeftForRemoveAndFree<T>(T[] array, int originalTruncationLength, int removeStart, int removeCount)
    {
        var copyStart = removeStart + removeCount;
        var length = originalTruncationLength - copyStart;
        array.AsSpan(copyStart, length).CopyTo(array.AsSpan(removeStart, length));
        FreeIfReferenceOrContainsReferences(array.AsSpan(removeStart + length, removeCount));
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

    #endregion

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
        Debug.Assert(expectedLength > array.Length);
        array = new T[GetNewLength(array.Length, expectedLength)];
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

    internal static int GetNewLength(int length, int expectedLength)
    {
        Debug.Assert(length < expectedLength);

        int newLen;
        var maxLength = ArrayMaxLengthProvider.MaxLength;
        if (length == 0)
            newLen = 4;
        else
            newLen = Math.Min(length * 2, maxLength);

        if (newLen < expectedLength)
            newLen = expectedLength;
        return newLen;
    }

    internal static int GetNewLength(int length, int expectedLength, int maxLength)
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
