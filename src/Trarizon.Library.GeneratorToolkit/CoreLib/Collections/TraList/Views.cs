namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
partial class TraList
{
    public static Memory<T> AsMemory<T>(this List<T> list)
        => Utils<T>.GetUnderlyingArray(list).AsMemory(list.Count);

    public static Span<T> AsSpan<T>(this List<T> list)
        => Utils<T>.GetUnderlyingArray(list).AsSpan(list.Count);
}
