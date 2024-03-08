using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Helpers;

namespace Trarizon.Library.Collections.Helpers;
public static partial class ListHelper
{
    public static List<T> Repeat<T>(T item, int count)
    {
        var list = new List<T>(count);
        CollectionsMarshal.SetCount(list, count);
        list.Fill(item);
        return list;
    }
}
