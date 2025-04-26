using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
public static partial class TraArray
{
    public static bool TryAt<T>(this ImmutableArray<T> array, int index, [MaybeNullWhen(false)] out T element)
    {
        if (array.IsDefault || index < 0 || index >= array.Length) {
            element = default!;
            return false;
        }
        element = array[index];
        return true;
    }
}
