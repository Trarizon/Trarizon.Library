using System.Runtime.CompilerServices;
using Trarizon.Library.Collections.Extensions.ListQueries.Helpers;

namespace Trarizon.Library.Collections.Extensions;
public static partial class ListQuery
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ListWrapper<T> Wrap<T>(this IList<T> list) => Unsafe.As<IList<T>, ListWrapper<T>>(ref list);
}
