using System.Collections.Immutable;

namespace Trarizon.Library.Collections.Helpers;
partial class ArrayHelper
{
    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> array) => array.IsDefault ? [] : array;

    public static T[] EmptyIfNull<T>(this T[]? array) => array ?? [];
}
