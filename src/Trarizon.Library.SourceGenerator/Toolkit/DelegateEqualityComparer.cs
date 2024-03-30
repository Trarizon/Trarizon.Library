using System;
using System.Collections.Generic;
using System.Text;

namespace Trarizon.Library.SourceGenerator.Toolkit;
internal class DelegateEqualityComparer<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
{
    public bool Equals(T x, T y) => equals(x, y);
    public int GetHashCode(T obj) => getHashCode(obj);
}
