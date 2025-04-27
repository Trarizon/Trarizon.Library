namespace Trarizon.Library.Collections.Helpers;
#if NETSTANDARD2_0
internal static class PfRuntimeHelpers
{
    public static bool IsReferenceOrContainsReferences<T>()
    {
        return !typeof(T).IsPrimitive;
    }
}
#endif
