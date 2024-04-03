using System.Collections.Generic;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class CollectionExtensions
{
    public static void SafelyAdd<T>(this List<T>? list,T item)
    {
        list ??= [];
        list.Add(item);
    }
}
