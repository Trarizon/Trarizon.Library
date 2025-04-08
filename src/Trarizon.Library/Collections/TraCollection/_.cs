using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
    #region NetStd_Fix

#if NETSTANDARD2_0

    public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T value)
    {
        if (stack.Count == 0) {
            value = default;
            return false;
        }
        value = stack.Pop();
        return true;
    }

    public static bool TryPeek<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T value)
    {
        if (stack.Count == 0) {
            value = default;
            return false;
        }
        value = stack.Peek();
        return true;
    }

    public static bool TryDequeue<T>(this Queue<T> queue, [MaybeNullWhen(false)] out T value)
    {
        if (queue.Count == 0) {
            value = default;
            return false;
        }
        value = queue.Dequeue();
        return true;
    }

#endif

    #endregion

    public static class Utils<T>
    {
        #region List

#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_version")]
        public static extern ref int GetVersion(List<T> list);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        public static extern ref T[] GetUnderlyingArray(List<T> list);
#else
        public static ref T[] GetUnderlyingArray(List<T> list) 
            => ref Unsafe.As<List<T>, StrongBox<T[]>>(ref list).Value!;
#endif

        #endregion

        #region Stack

#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_array")]
        public static extern ref T[] GetUnderlyingArray(Stack<T> stack);
#else
        public static ref T[] GetUnderlyingArray(Stack<T> stack)
            => ref Unsafe.As<StackMarchalHelper>(stack)._array!;

        private class StackMarchalHelper
        {
#nullable disable
            public T[] _array;
            public int _size;
            public int _version;
#nullable restore
        }
#endif

        #endregion
    }
}
