// Extends Enumerable.AtOrDefault which
// - does not check IReadOnlyList<T>
// - does not have TryAt
// https://source.dot.net/#System.Linq/System/Linq/ElementAt.cs,0bd04e5c179c3c7d

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Collections.Helpers;
partial class ListHelper
{
    public static ref T AtRef<T>(this List<T> list, int index)
        => ref CollectionsMarshal.AsSpan(list)[index];


    /// <summary>
    /// Get the item at <paramref name="index"/>,
    /// if <paramref name="index"/> is out of range, return <paramref name="defaultValue"/>
    /// </summary>
    public static T? ElementAtOrDefaultROList<T>(this IReadOnlyList<T> list, Index index, T defaultValue = default!)
    {
        if (list.TryAtROList(index, out var value))
            return value;
        return defaultValue;
    }


    /// <summary>
    /// Try get the item at <paramref name="index"/>
    /// </summary>
    public static bool TryAt<T>(this IList<T> list, Index index, [MaybeNullWhen(false)] out T value)
        => TryAtOpt(list, index.GetOffset(list.Count), out value);

    /// <summary>
    /// Try get the item at <paramref name="index"/>
    /// </summary>
    public static bool TryAtROList<T>(this IReadOnlyList<T> list, Index index, [MaybeNullWhen(false)] out T value)
        => TryAtOpt(list.Wrap(), index.GetOffset(list.Count), out value);

    /// <summary>
    /// Try get the item at <paramref name="index"/>
    /// </summary>
    public static bool TryAt<T>(this IList<T> list, int index, [MaybeNullWhen(false)] out T value)
        => TryAtOpt(list, index, out value);

    /// <summary>
    /// Try get the item at <paramref name="index"/>
    /// </summary>
    public static bool TryAtROList<T>(this IReadOnlyList<T> list, int index, [MaybeNullWhen(false)] out T value)
        => TryAtOpt(list.Wrap(), index, out value);

    #region Internal

    internal static bool TryAtOpt<TList, T>(TList list, int index, out T? value) where TList : IList<T>
    {
        var count = list.Count;
        if (index < count && index >= 0) {
            value = list[index];
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    #endregion
}
