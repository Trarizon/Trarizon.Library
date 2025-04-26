using System.Buffers;
using System.Diagnostics;

namespace Trarizon.Library.Collections.Helpers;
internal static partial class ArrayGrowHelper
{
    public static void GrowPooled<T>(ref T[] array, int expectedLength, int copyLength, bool clearOriginalArray = false)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(copyLength <= array.Length);

        var original = array;
        array = ArrayPool<T>.Shared.Rent(expectedLength);
        original.AsSpan().CopyTo(array);
        ArrayPool<T>.Shared.Return(original, clearOriginalArray);
    }

    public static void GrowPooledForInsertion<T>(ref T[] array, int expectedLength, int copyCount, int insertIndex, int insertCount, bool clearOriginalArary = false)
    {
        var originalArray = array;
        array = ArrayPool<T>.Shared.Rent(expectedLength);
        if (insertIndex > 0) {
            originalArray.AsSpan(0, insertIndex).CopyTo(array);
        }
        if (insertIndex < copyCount) {
            var count = copyCount - insertIndex;
            originalArray.AsSpan(insertIndex, count).CopyTo(array.AsSpan(insertIndex + insertCount, count));
        }
        ArrayPool<T>.Shared.Return(originalArray, clearOriginalArary);
    }
}
