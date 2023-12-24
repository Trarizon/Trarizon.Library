using Microsoft.CodeAnalysis;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    public static bool HasGeneratedCodeAttribute(this ISymbol symbol)
        => symbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == Literals.GeneratedCodeAttributeFullName);

    public static bool MatchMetadataString(this ISymbol symbol, string metadataStringWithArity)
    {
        var displayStr = symbol.ToDisplayString();
        if (displayStr[^1] == '>') {
            // count till
            int index = displayStr.Length - 2;
            int genericParameterCount = 0;
            for (; index >= 0; index--) {
                var c = displayStr[index];
                if (c == ',')
                    genericParameterCount++;
                else if (c == '<')
                    break;
            }
            displayStr = $"{displayStr[..index]}`{genericParameterCount}";
        }
        return displayStr == metadataStringWithArity;
    }
}
