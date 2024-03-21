using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Trarizon.Library.SourceGenerator.Toolkit.More;
public static class MoreSymbolEqualityComaprer
{
    public static readonly IEqualityComparer<ISymbol?> OriginalDefination = new OriginalDefinationComparer();

    private class OriginalDefinationComparer : IEqualityComparer<ISymbol?>
    {
        public bool Equals(ISymbol? x, ISymbol? y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition);
        public int GetHashCode(ISymbol? obj) => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition);
    }
}
