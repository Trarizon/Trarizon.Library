using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class SymbolExtensions
{
    [field: MaybeNull]
    public static IEqualityComparer<ISymbol?> OriginalDefinationEqualityComparer => field ??= TraComparison.CreateEqualityComparer<ISymbol?>(
        (x, y) => SymbolEqualityComparer.Default.Equals(x?.OriginalDefinition, y?.OriginalDefinition),
        obj => SymbolEqualityComparer.Default.GetHashCode(obj?.OriginalDefinition));

    /// <remarks>
    /// eg: <c>List&lt;T></c> as <c>System.Collections.Generic.List</c>
    /// </remarks>
    [field: MaybeNull]
    public static SymbolDisplayFormat DefaultDisplayFormatWithouGeneric => field ??= SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);

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

    #region RuntimeHelpers

    public static bool IsRuntimeType(this ITypeSymbol symbol, Type runtimeType, Compilation compilation, bool includeNullability = false)
    {
        var comparer = includeNullability ? SymbolEqualityComparer.IncludeNullability : SymbolEqualityComparer.Default;
        return comparer.Equals(symbol, compilation.GetTypeSymbolByRuntimeType(runtimeType));
    }

    public static bool IsRuntimeType<T>(this ITypeSymbol symbol, Compilation compilation, bool includeNullability = false)
        => symbol.IsRuntimeType(typeof(T), compilation, includeNullability);

    #endregion
}
