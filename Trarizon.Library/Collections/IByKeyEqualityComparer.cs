using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Collections;
internal interface IByKeyEqualityComparer<T, TKey>
{
    bool Equals(T val, TKey key);
    int GetHashCode([DisallowNull] T val);
    int GetHashCode([DisallowNull] TKey key);
}
