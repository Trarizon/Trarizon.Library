using System.ComponentModel;

namespace Trarizon.Library.Collections.Comparers;

partial class TraComparers
{
    extension<T>(EqualityComparer<T>)
    {
#if NET8_0_OR_GREATER
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use Official EqualityComparer<T>.Create Instead")]
#endif
        public static IEqualityComparer<T> Create(Func<T?, T?, bool> equals, Func<T, int> getHashCode)
            => new DelegateEqualityComparer<T>(equals, getHashCode);
    }

    private sealed class DelegateEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y) => equals(x, y);
        public int GetHashCode(T obj) => getHashCode(obj);
    }
}
