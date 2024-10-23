using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraComparison
{
    public static ReversedComparer<T, IComparer<T>> Reverse<T>(this IComparer<T> comparer) => new(comparer);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct ReversedComparer<T, TComparer>(TComparer comparer) : IComparer<T> where TComparer : IComparer<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T? x, T? y) => comparer.Compare(y, x);
    }
}
