namespace Trarizon.Library.Collections;
internal static class EnumerationHelper
{
    public static bool MoveNext_ArrayPrefix<T>(T[] array, int count, ref int index, out T? current)
    {
        if (index < 0) {
            current = default;
            return false;
        }

        if (index < count) {
            current = array[index];
            index++;
            return true;
        }
        else {
            index = -1;
            current = default!;
            return false;
        }
    }

    public static void Reset_ArrayPrefix<T>(ref int index)
        => index = 0;
}
