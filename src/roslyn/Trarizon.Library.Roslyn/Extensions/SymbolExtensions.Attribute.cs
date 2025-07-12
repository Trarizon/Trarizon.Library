using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class SymbolExtensions
{
    public static bool TryGetAttributeData([NotNullWhen(true)] this ISymbol? symbol, string attributeTypeFullName, [NotNullWhen(true)] out AttributeData? attribute)
    {
        if (symbol is null) {
            attribute = null;
            return false;
        }
        attribute = symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.MatchDisplayString(attributeTypeFullName));
        return attribute is not null;
    }

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, string? tool, string? version, [NotNullWhen(true)] out AttributeData? attribute)
    {
        attribute = null;

        if (!KnownLiterals.GeneratedCodeAttributeProxy.TryGet(symbol, out var attrProxy))
            return false;
        if (tool is not null && tool != attrProxy.Tool)
            return false;
        if (version is not null && version != attrProxy.Version)
            return false;

        attribute = attrProxy.AttributeData;
        return true;
    }
}
