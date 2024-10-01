using System.Runtime.CompilerServices;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
public static partial class TraList
{
    private static class Utils<T>
    {
        public static ref T[] GetUnderlyingArray(List<T> list)
        {
            var provider = Unsafe.As<StrongBox<T[]>>(list);
            return ref provider.Value!;
        }
    }
}
