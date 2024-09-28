using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
partial class TraEnumerable
{
    public static bool TryAt<T>(this IEnumerable<T> source, int index, [MaybeNullWhen(false)] out T element)
    {
        if (index < 0) {
            element = default;
            return false;
        }

        if (source is IList<T> list) {
            if (index >= list.Count) {
                element = default;
                return false;
            }
            else {
                element = list[index];
                return true;
            }
        }

        using var enumerator = source.GetEnumerator();
        if (enumerator.TryIterate(index, out _)) {
            element = enumerator.Current;
            return true;
        }
        else {
            element = default;
            return false;
        }
    }
}
