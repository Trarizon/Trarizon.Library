namespace Trarizon.Library.GeneratorToolkit.CoreLib.Collections;
public static class TraComparer
{
    public static IEqualityComparer<T> CreateEquality<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
        => new DelegateEqualityComparer<T>(equals, getHashCode);

    private sealed class DelegateEqualityComparer<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
    {
        public bool Equals(T x, T y) => equals(x, y);
        public int GetHashCode(T obj) => getHashCode(obj);
    }
}
