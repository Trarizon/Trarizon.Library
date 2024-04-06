using System.Collections.Immutable;

namespace Trarizon.Library.Collections.Helpers;
partial class ArrayHelper
{
    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> array)
        => array.IsDefault ? [] : array;
}
