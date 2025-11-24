using System.Collections;

namespace Trarizon.Library.Linq;

public static partial class TraEnumerable
{
    public static IEnumerable<T> OfTypeWhile<T>(this IEnumerable source)
    {
        if (default(T) is not null && source is IEnumerable<T> typed) {
            return typed;
        }
        return Iterate(source);

        static IEnumerable<T> Iterate(IEnumerable source)
        {
            foreach (var item in source) {
                if (item is T t)
                    yield return t;
                else
                    yield break;
            }
        }
    }
}
