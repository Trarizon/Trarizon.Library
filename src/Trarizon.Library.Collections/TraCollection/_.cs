using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraCollection
{
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
