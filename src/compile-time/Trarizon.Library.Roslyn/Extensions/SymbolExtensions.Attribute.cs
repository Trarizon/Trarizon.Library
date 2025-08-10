using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Roslyn.SourceInfos;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class SymbolExtensions
{
    public static bool TryGetAttributeData(this ISymbol symbol, INamedTypeSymbol attributeType, [MaybeNullWhen(false)] out AttributeData attributeData)
    {
        foreach (var attr in symbol.GetAttributes()) {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType)) {
                attributeData = attr;
                return true;
            }
        }
        attributeData = null;
        return false;
    }

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeType) 
        => symbol.TryGetAttributeData(attributeType, out _);

    public static bool TryGetAttributeData(this ISymbol symbol, string attributeTypeFullName, [NotNullWhen(true)] out AttributeData? attributeData)
    {
        foreach (var attr in symbol.GetAttributes()) {
            if (attr.AttributeClass?.ToDisplayString() == attributeTypeFullName) {
                attributeData = attr;
                return true;
            }
        }
        attributeData = null;
        return false;
    }

    public static bool HasAttribute(this ISymbol symbol, string attributeTypeFullName) 
        => symbol.TryGetAttributeData(attributeTypeFullName, out _);

    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol, Functional.Optional<string> tool, Functional.Optional<string> version, [NotNullWhen(true)] out AttributeData? attribute)
    {
        attribute = null;

        if (!symbol.TryGetAttributeData(KnownInfos.GeneratedCodeAttribute.TypeFullName, out var attr))
            return false;

        if (tool.TryGetValue(out var vtool) && vtool != attr.GetConstructorArgument(0).Cast<string>())
            return false;
        if (version.TryGetValue(out var vver) && vver != attr.GetConstructorArgument(1).Cast<string>())
            return false;

        attribute = attr;
        return true;
    }
}
