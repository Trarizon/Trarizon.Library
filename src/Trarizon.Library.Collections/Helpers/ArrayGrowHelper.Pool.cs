using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_0
using RuntimeHelpers = Trarizon.Library.Collections.Helpers.PfRuntimeHelpers;
#endif

namespace Trarizon.Library.Collections.Helpers;
internal static partial class ArrayGrowHelper
{
    public static void GrowPooled<T>(ref T[] array, int expectedLength, int copyLength)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(copyLength <= array.Length);

        var original = array;
        array = ArrayPool<T>.Shared.Rent(expectedLength);
        original.AsSpan().CopyTo(array);
        ArrayPool<T>.Shared.Return(original, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    public static void GrowPooledForInsertion<T>(ref T[] array, int expectedLength, int copyCount, int insertIndex, int insertCount)
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
        ArrayPool<T>.Shared.Return(originalArray, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }
}
