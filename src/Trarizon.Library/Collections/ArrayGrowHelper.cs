﻿using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
internal static class ArrayGrowHelper
{
    public static void FreeManaged<T>(T[] array)
    {
#if !NETSTANDARD2_0
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
#endif
        Array.Clear(array, 0, array.Length);
    }

    public static void FreeManaged<T>(T[] array, Range range)
    {
#if !NETSTANDARD2_0
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif

        var (off, len) = range.GetOffsetAndLength(array.Length);
        Array.Clear(array, off, len);
    }

    public static void FreeManaged<T>(T[] array, int index, int length)
    {
#if !NETSTANDARD2_0
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif

        Array.Clear(array, index, length);
    }

    public static void Grow<T>(ref T[] array, int expectedLength, int copyLength)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(copyLength <= array.Length);

        var originalArray = array;
#if NETSTANDARD
        array = new T[GetNewLength(array.Length, expectedLength, TraArray.MaxLength)];
#else
        array = new T[GetNewLength(array.Length, expectedLength, Array.MaxLength)];
#endif
        Array.Copy(originalArray, array, copyLength);
    }

    public static void GrowNonMove<T>(ref T[] array, int expectedLength)
#if NETSTANDARD
        => GrowNonMove(ref array, expectedLength, TraArray.MaxLength);
#else
        => GrowNonMove(ref array, expectedLength, Array.MaxLength);
#endif
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
            Array.Copy(originalArray, array, insertIndex);
        }
        if (insertIndex < copyCount) {
            Array.Copy(originalArray, insertIndex, array, insertIndex + insertCount, copyCount - insertIndex);
        }
    }

    private static int GetNewLength(int length, int expectedLength, int maxLength)
    {
        Debug.Assert(length < expectedLength);

        int newLen;
        if (length == 0)
            newLen = 4;
        else
            newLen = Math.Min(length * 2, maxLength);

        if (newLen < expectedLength)
            newLen = expectedLength;
        return newLen;
    }
}
