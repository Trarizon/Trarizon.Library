﻿namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraEnumerable
{
    // LinQ optimized chained call Where().Select(), so we use official LinQ
    // for better performance on LinQ chain

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : class
        => source.Where(x => x is not null)!;

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : struct
        => source.Where(x => x is not null).Select(x => x!.Value);
}