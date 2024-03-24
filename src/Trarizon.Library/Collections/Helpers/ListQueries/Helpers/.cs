using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections.Helpers;
[SuppressMessage("Style", "IDE0301")] // Use explicit on Empty<T>(), collection will alloc new List<T> in some conditions
public static partial class ListQuery
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ListWrapper<T> Wrap<T>(this IList<T> list) => Unsafe.As<IList<T>, ListWrapper<T>>(ref list);
}
