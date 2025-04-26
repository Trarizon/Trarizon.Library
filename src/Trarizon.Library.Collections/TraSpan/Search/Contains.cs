namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static bool Contains<T, TEquatable>(this ReadOnlySpan<T> span, TEquatable value) where TEquatable : IEquatable<T>
    {
        foreach (var item in span) {
            if (value.Equals(item))
                return true;
        }
        return false;
    }
}
