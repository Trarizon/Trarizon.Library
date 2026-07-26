using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Trarizon.Library.Collections;

#if NETSTANDARD

internal static partial class Polyfills
{
    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is ICollection<T> collection)
        {
            count = collection.Count;
            return true;
        }
        if (source is ICollection ngcollection)
        {
            count = ngcollection.Count;
            return true;
        }
        count = 0;
        return false;
    }
}

#endif
