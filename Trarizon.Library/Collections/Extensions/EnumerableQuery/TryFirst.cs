using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Collections.Extensions;
partial class EnumerableQuery
{
    public static bool TryFirst<T>(this IEnumerable<T> source, out T value, T defaultValue = default!)
    {
        if (source is IList<T> list) {
            if (list.Count > 0) {
                value = list[0];
                return true;
            }
            else {
                value = defaultValue;
                return false;
            }
        }

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext()) {
            value = enumerator.Current;
            return true;
        }
        else {
            value = defaultValue;
            return false;
        }
    }
}
