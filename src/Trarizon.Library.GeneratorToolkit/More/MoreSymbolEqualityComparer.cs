using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Trarizon.Library.GeneratorToolkit.More;
public static class MoreSymbolEqualityComparer
{
    public static readonly IEqualityComparer<ISymbol?> ByOriginalDefination = new DelegateEqualityComparer<ISymbol?>(
        (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
        obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));
}
