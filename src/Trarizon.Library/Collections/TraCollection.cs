using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
#if NET9_0_OR_GREATER
    public static ReversedSpan<T> AsSpan<T>(Stack<T> stack) 
        => Utils<T>.GetUnderlyingArray(stack).AsSpan(..stack.Count).AsReversed();
#endif

#if NET9_0_OR_GREATER
    private static class Utils<T>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_array")]
        public static extern ref T[] GetUnderlyingArray(Stack<T> stack);
    }
#endif
}
