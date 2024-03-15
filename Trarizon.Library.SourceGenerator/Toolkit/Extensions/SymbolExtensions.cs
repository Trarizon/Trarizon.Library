using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Toolkit.Extensions;
public static class SymbolExtensions
{
    public static bool TryGetGeneratedCodeAttribute(this ISymbol symbol,[NotNullWhen(true)] out AttributeData attributeData)
        => symbol.GetAttributes()
            .TryFirst(attr => attr!.AttributeClass.MatchDisplayString(GlobalLiterals.GeneratedCodeAttribute_TypeName), out attributeData);

    public static bool MatchDisplayString(this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
        => symbol?.ToDisplayString(symbolDisplayFormat) == displayString;

    public static string ToCsFileNameString(this ISymbol symbol, string suffix)
    {
        var display = symbol.ToDisplayString();
        var builder = (stackalloc char[display.Length + 4 + suffix.Length]);
        display.AsSpan().CopyTo(builder);
        builder[display.Length] = '.';
        suffix.AsSpan().CopyTo(builder[(display.Length + 1)..]);
        foreach (ref var c in builder) {
            switch (c) {
                case '<': c = '{'; break;
                case '>': c = '}'; break;
            }
        }
        builder[^3] = '.';
        builder[^2] = 'c';
        builder[^1] = 's';
        return builder.ToString();
    }
}
