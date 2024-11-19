using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraList
{
    private static class Utils<T>
    {
#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_version")]
        public static extern ref int GetVersion(List<T> list);
#endif
    }
}
