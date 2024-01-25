using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Extensions;
public static partial class ListExtensions
{
    public static void Fill<T>(this List<T> list, T item)
        => CollectionsMarshal.AsSpan(list).Fill(item);

    public static void SortStably<T>(this List<T> list, Comparison<T>? comparison = null)
        => CollectionsMarshal.AsSpan(list).SortStably(comparison);
}
