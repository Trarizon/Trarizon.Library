using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Library.Collections.Creators;
public static partial class ListCreator
{
    public static List<T> Repeat<T>(T item, int count)
    {
        var list = new List<T>(count);
        CollectionsMarshal.SetCount(list, count);
        list.Fill(item);
        return list;
    }
}
