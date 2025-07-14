using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class SymbolExtensions
{
    private static IEqualityComparer<ISymbol?>? _originalDefinationSymbolEqualityComparer;

    extension(SymbolEqualityComparer)
    {
        public static IEqualityComparer<ISymbol?> OriginalDefination => _originalDefinationSymbolEqualityComparer ??= TraComparison.CreateEqualityComparer<ISymbol?>(
            (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
            obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));
    }

    private static SymbolDisplayFormat? _defaultWithGenericSymbolDisplayFormat;

    extension(SymbolDisplayFormat)
    {
        /// <remarks>
        /// eg: <c>List&lt;T></c> as <c>System.Collections.Generic.List</c>
        /// </remarks>
        public static SymbolDisplayFormat DefaultWithoutGenerics => _defaultWithGenericSymbolDisplayFormat ??=
             SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);
    }

    public static IEnumerable<ISymbol> ContainingSymbols(this ISymbol symbol, bool includeSelf = false)
    {
        if (includeSelf)
            yield return symbol;

        var sym = symbol.ContainingSymbol;
        while (sym is not null) {
            yield return sym;
            sym = sym.ContainingSymbol;
        }
    }
}
