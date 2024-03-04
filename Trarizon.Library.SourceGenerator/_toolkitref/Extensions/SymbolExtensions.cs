using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    public static bool MatchDisplayString(this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
        => symbol?.ToDisplayString(symbolDisplayFormat) == displayString;
}
