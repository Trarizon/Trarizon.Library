using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static class SymbolExt
{
    public static IEqualityComparer<ISymbol?> OriginalDefinationEqualityComparer => field ??= TraComparison.CreateEqualityComparer<ISymbol?>(
        (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
        obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));

    public static bool MatchDisplayString([NotNullWhen(true)] this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
    {
        if (symbol is null)
            return false;
        return symbol.ToDisplayString(symbolDisplayFormat) == displayString;
    }

    public static IEnumerable<ISymbol> ContainingSymbols(this ISymbol symbol)
    {
        var sym = symbol.ContainingSymbol;
        while (sym is not null) {
            yield return sym;
            sym = sym.ContainingSymbol;
        }
    }
}
