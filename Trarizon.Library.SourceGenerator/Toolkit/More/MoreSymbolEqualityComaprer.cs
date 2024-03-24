using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Trarizon.Library.SourceGenerator.Toolkit.More;
public class MoreSymbolEqualityComaprer
{
    public static readonly IEqualityComparer<ISymbol?> OriginalDefination = new DelegateEqualityComaprer<ISymbol?>(
        (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
        obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));

    private class DelegateEqualityComaprer<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
    {
        public bool Equals(T x, T y) => equals(x, y);
        public int GetHashCode(T obj) => getHashCode(obj);
    }
}
