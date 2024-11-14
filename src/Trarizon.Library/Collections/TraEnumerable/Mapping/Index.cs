#if NET9_0_OR_GREATER
using System.ComponentModel;
#endif

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
#if NET9_0_OR_GREATER
    [EditorBrowsable(EditorBrowsableState.Never)]
#endif
    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
    {
        if (source.IsEmptyArray())
            return [];
        return Iterate(source);

        static IEnumerable<(int, T)> Iterate(IEnumerable<T> source)
        {
            int i = 0;
            foreach (var item in source) {
                yield return (i++, item);
            }
        }
    }
}
