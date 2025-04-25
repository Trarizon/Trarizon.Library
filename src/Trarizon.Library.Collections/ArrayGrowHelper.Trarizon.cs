using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
internal static partial class ArrayGrowHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FreeManaged<T>(ConcatSpan<T> span)
    {
#if !NETSTANDARD2_0
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;
#endif
        span.First.Clear();
        span.Second.Clear();
    }
}
