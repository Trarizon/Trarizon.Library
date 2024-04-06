using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    public static void Fill<T>(this List<T> list, T item)
        => CollectionsMarshal.AsSpan(list).Fill(item);
}
