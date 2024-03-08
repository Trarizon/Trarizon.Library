namespace Trarizon.Library.Collections.Helpers;
public static class EnumerableHelper
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) {
            action(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        int i = 0;
        foreach (var item in source) {
            action(item, i++);
        }
    }
}
