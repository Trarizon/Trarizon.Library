using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections;

#if NETSTANDARD

internal static partial class Polyfills
{
    public static void EnsureCapacity<T>(this List<T> list, int expectedCapacity)
    {
        if (expectedCapacity <= list.Capacity)
            return;
        list.Capacity = ArrayGrowHelper.GetNewLength(list.Capacity, expectedCapacity);
    }
}

#endif
