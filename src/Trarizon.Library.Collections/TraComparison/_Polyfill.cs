namespace Trarizon.Library.Collections;
public static partial class TraComparison
{
#if NETSTANDARD

    extension<T>(EqualityComparer<T>)
    {
        public static IEqualityComparer<T> Create(Func<T?, T?, bool> equals, Func<T, int> getHashCode)
            => new DelegateEqualityComparer<T>(equals, getHashCode);
    }
    private sealed class DelegateEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y) => equals(x, y);
        public int GetHashCode(T obj) => getHashCode(obj);
    }

#endif
}
