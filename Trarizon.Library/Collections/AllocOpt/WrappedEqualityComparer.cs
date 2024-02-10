using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections.AllocOpt;
public readonly struct WrappedEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly IEqualityComparer<T>? _comparer;

    public bool Equals(T? x, T? y)
    {
        if (typeof(T).IsValueType && _comparer is null)
            return EqualityComparer<T>.Default.Equals(x, y);
        else
            return (_comparer ?? EqualityComparer<T>.Default).Equals(x, y);
    }

    public int GetHashCode([DisallowNull] T obj)
    {
        if (typeof(T).IsValueType && _comparer is null)
            return obj.GetHashCode();
        else
            return (_comparer ?? EqualityComparer<T>.Default).GetHashCode(obj);
    }
}
