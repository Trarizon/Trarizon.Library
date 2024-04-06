namespace Trarizon.Library.Collections.Helpers;
partial class ArrayHelper
{
    public static void Fill<T>(this T[] array, T item)
        => array.AsSpan().Fill(item);
}
