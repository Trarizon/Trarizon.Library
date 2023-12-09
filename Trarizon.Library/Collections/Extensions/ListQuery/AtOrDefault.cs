namespace Trarizon.Library.Collections.Extensions;
partial class ListQuery
{
    /// <summary>
    /// Get the item at <paramref name="index"/>,
    /// if <paramref name="index"/> is out of range, return <paramref name="defaultValue"/>
    /// </summary>
    public static T AtOrDefault<T>(this IList<T> list, int index, T defaultValue = default!) 
        => index >= list.Count || index < 0 ? defaultValue : list[index];

    /// <summary>
    /// Get the item at <paramref name="index"/>,
    /// if <paramref name="index"/> is out of range, return <paramref name="defaultValue"/>
    /// </summary>
    public static T AtOrDefault<T>(this IList<T> list, Index index, T defaultValue = default!)
        => index.Value >= list.Count ? defaultValue : list[index];
}
