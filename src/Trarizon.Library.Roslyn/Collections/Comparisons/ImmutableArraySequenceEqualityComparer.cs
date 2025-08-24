using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Trarizon.Library.Roslyn.Collections.Comparisons;
public sealed class ImmutableArraySequenceEqualityComparer<T> : IEqualityComparer<ImmutableArray<T>>
{
    private ImmutableArraySequenceEqualityComparer() { }

    public static ImmutableArraySequenceEqualityComparer<T> Default { get; } = new();

    public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y)
    {
        if (x.Length != y.Length)
            return false;
        for (int i = 0; i < x.Length; i++) {
            if (!EqualityComparer<T>.Default.Equals(x[i], y[i]))
                return false;
        }
        return true;
    }

    public int GetHashCode(ImmutableArray<T> obj)
    {
        HashCode hc = new();
        for (int i = 0; i < obj.Length; i++) {
            hc.Add(obj);
        }
        return hc.ToHashCode();
    }
}
