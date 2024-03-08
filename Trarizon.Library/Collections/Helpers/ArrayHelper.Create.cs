namespace Trarizon.Library.Collections.Helpers;
public static partial class ArrayHelper
{
    public static T[] Repeat<T>(T item, int length)
    {
        var res = new T[length];
        res.Fill(item);
        return res;
    }
}
