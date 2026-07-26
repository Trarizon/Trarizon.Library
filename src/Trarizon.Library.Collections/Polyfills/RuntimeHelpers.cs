using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;

#if NETSTANDARD2_0

internal static partial class Polyfills
{
    extension(RuntimeHelpers)
    {
        public static bool IsReferenceOrContainsReferences<T>()
        {
            return !typeof(T).IsPrimitive;
        }
    }
}

#endif
