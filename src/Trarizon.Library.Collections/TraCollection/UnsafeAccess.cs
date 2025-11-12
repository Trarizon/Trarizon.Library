using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;

public static partial class TraCollection
{
    internal static class UnsafeAccess<T>
    {
        #region List

#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_version")]
        public static extern ref int GetVersion(List<T> list);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        public static extern ref T[] GetItems(List<T> list);
#else
        public static ref T[] GetItems(List<T> list)
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
#pragma warning disable CS0649
#nullable disable
            public T[] _array;
            public int _size;
            public int _version;
#nullable restore
#pragma warning restore CS0649
        }
#endif

        #endregion
    }
}
