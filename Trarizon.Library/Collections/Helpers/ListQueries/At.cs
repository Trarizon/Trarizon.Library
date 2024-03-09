// Extends Enumerable.AtOrDefault which
// - does not check IReadOnlyList<T>
// - does not have TryAt
// https://source.dot.net/#System.Linq/System/Linq/ElementAt.cs,0bd04e5c179c3c7d

using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.Helpers;
partial class ListQuery
{
    /// <summary>
    /// Get the item at <paramref name="index"/>,
    /// if <paramref name="index"/> is out of range, return <paramref name="defaultValue"/>
    /// </summary>
    public static T ElementAtOrDefaultROList<T>(this IReadOnlyList<T> list, Index index, T defaultValue = default!)
        => ElementAtOrDefaultOpt(list, index, defaultValue);


    /// <summary>
    /// Try get the item at <paramref name="index"/>
    /// </summary>
    public static bool TryAt<T>(this IList<T> list, Index index, [MaybeNullWhen(false)] out T value)
        => TryAtOpt(list.Wrap(), index, out value);

    /// <summary>
    /// Try get the item at <paramref name="index"/>
    /// </summary>
    public static bool TryAtROList<T>(this IReadOnlyList<T> list, Index index, [MaybeNullWhen(false)] out T value)
        => TryAtOpt(list, index, out value);


    #region Internal

    internal static T ElementAtOrDefaultOpt<TList, T>(TList list, Index index, T defaultValue = default!) where TList : IReadOnlyList<T>
    {
        var count = list.Count;
        var i = index.Value;
        if (i >= 0 && i < count) {
            return list[index.GetOffset(count)];
        }
        else {
            return defaultValue;
        }
    }

    internal static bool TryAtOpt<TList, T>(TList list, Index index, out T? value) where TList : IReadOnlyList<T>
    {
        var count = list.Count;
        var i = index.Value;
        if (i >= 0 && i < count) {
            value = list[index.GetOffset(count)];
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    #endregion
}
