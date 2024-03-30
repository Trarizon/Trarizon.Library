using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Trarizon.Library.SourceGenerator.Toolkit.More;
public class MoreSymbolEqualityComaprer
{
    public static readonly IEqualityComparer<ISymbol?> OriginalDefination = new DelegateEqualityComparer<ISymbol?>(
        (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
        obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));
}
