using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Library.Collections.Creators;
public static partial class ArrayCreator
{
    public static T[] Repeat<T>(T item, int length)
    {
        var res = new T[length];
        res.Fill(item);
        return res;
    }
}
