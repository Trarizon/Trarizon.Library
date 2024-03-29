﻿namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    public static IList<T> AsList<T>(this IList<T> list) => list;

    public static IReadOnlyList<T> AsReadOnlyList<T>(this IReadOnlyList<T> list) => list;
}
