using System;
using System.Collections.Generic;

namespace Trarizon.Library.GeneratorToolkit;
public sealed class DelegateEqualityComparer<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
{
    public bool Equals(T x, T y) => equals(x, y);
    public int GetHashCode(T obj) => getHashCode(obj);
}
