using System.Runtime.CompilerServices;

namespace Trarizon.Library.Common.Polyfills;
internal static class PfRuntimeHelpers
{
    public static bool IsReferenceOrContainsReferences<T>()
    {
#if NETSTANDARD2_0
        return !typeof(T).IsPrimitive;
#else
        return RuntimeHelpers.IsReferenceOrContainsReferences<T>();
#endif
    }
}
