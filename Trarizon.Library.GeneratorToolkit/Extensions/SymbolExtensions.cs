using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    public static bool MatchDisplayString(this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
        => symbol?.ToDisplayString(symbolDisplayFormat) == displayString;

    public static bool HasGeneratedCodeAttribute(this ISymbol symbol)
        => symbol.GetAttributes().Any(attr => attr.AttributeClass.MatchDisplayString(Literals.GeneratedCodeAttributeFullName));

    public static string ToFullMetadataDisplayString(this ISymbol symbol)
    {
        if (symbol is null)
            return string.Empty;

        if (symbol is INamespaceSymbol { IsGlobalNamespace: true })
            return string.Empty;

        Stack<(char, string)> names = new();
        int strLength = 0;

        do {
            var containg = symbol.ContainingSymbol;
            var item = (
                containg is ITypeSymbol && symbol is ITypeSymbol ? '+' : '.',
                symbol.MetadataName);

            names.Push(item);
            strLength += item.MetadataName.Length + 1;

            symbol = containg;
        } while (symbol is not INamespaceSymbol { IsGlobalNamespace: true });

        var result = (stackalloc char[strLength]);
        return result.ToString();
    }
}
