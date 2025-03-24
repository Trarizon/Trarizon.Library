using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.StackAlloc;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    public static ReversedSpan<T> AsSpan<T>(Stack<T> stack)
        => Utils<T>.GetUnderlyingArray(stack).AsSpan(..stack.Count).AsReversed();

    private static class Utils<T>
    {
#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_array")]
        public static extern ref T[] GetUnderlyingArray(Stack<T> stack);
#else
        public static ref T[] GetUnderlyingArray(Stack<T> stack)
            => ref Unsafe.As<StackMarchalHelper>(stack)._array!;

        private class StackMarchalHelper
        {
            public T[]? _array;
            public int _size;
            public int _version;
        }
#endif
    }

}
