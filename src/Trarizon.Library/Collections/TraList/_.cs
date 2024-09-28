#if NET9_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Trarizon.Library.Collections;
public static partial class TraList
{
#if NET9_0_OR_GREATER
    public static class Utils<T>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        public static extern ref T[] GetUnderlyingArray(List<T> list);
    }
#endif
}
