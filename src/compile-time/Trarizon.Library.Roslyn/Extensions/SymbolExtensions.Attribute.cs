using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Roslyn.SourceInfos;

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
