#if NET9_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraList
{
    private static class Utils<T>
    {
#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        public static extern ref T[] GetUnderlyingArray(List<T> list);
#else
        public static ref T[] GetUnderlyingArray(List<T> list)
        {
            var provider = Unsafe.As<StrongBox<T[]>>(list);
            return ref provider.Value!;
        }
#endif
    }
}
