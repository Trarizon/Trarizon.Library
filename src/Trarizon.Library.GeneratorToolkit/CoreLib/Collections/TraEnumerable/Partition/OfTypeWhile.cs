using System.Collections;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    public static IEnumerable<T> OfTypeWhile<T>(this IEnumerable source)
    {
        if (source is IEnumerable<T> typed) {
            return typed;
        }
        return Iterate();

        IEnumerable<T> Iterate()
        {
            foreach (var item in source) {
                if (item is T t)
                    yield return t;
                else
                    yield break;
            }
        }
    }

    public static IEnumerable<T> OfTypeUntil<T, TExcept>(this IEnumerable<T> source) where TExcept : T
    {
        foreach (var item in source) {
            if (item is TExcept)
                yield break;
            else
                yield return item;
        }
    }
}
