using System.Runtime.InteropServices;
using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Library.Collections.Creators;
public static partial class ListCreator
{
    public static List<T> Repeat<T>(int count, T item)
    {
        var list = new List<T>(count);
#if NET8_0_OR_GREATER
        CollectionsMarshal.SetCount(list, count);
        list.Fill(item);
#else
        for (int i = 0; i < count; i++)
            list.Add(item);
#endif
        return list;
    }
}
