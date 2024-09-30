using Microsoft.CodeAnalysis;
using Trarizon.Library.GeneratorToolkit.CoreLib.Collections;

namespace Trarizon.Library.GeneratorToolkit.More;
public static class MoreSymbolEqualityComparer
{
    public static readonly IEqualityComparer<ISymbol?> ByOriginalDefination = TraComparer.CreateEquality<ISymbol?>(
        (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
        obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));
}
