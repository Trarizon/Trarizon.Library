using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static partial class SymbolExtensions
{
    public static bool MatchDisplayString([NotNullWhen(true)] this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
    {
        if (symbol is null)
            return false;
        return symbol.ToDisplayString(symbolDisplayFormat) == displayString;
    }

    public static string ToValidFileNameString(this ISymbol symbol)
    {
        var display = symbol.ToDisplayString();
        var builder = (stackalloc char[display.Length]);
        display.AsSpan().CopyTo(builder);
        foreach (ref var c in builder) {
            if (c is '<') c = '{';
            if (c is '>') c = '}';
        }
        return builder.ToString();
    }

    /// <summary>
    /// Return the full qualified display string, include prefix <c>global::</c>
    /// </summary>
    public static string ToFullQualifiedDisplayString(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <returns><see langword="null"/> if <paramref name="symbol"/> is global namespace</returns>
    public static string? ToNonGlobalDisplayString(this INamespaceSymbol symbol)
    {
        if (symbol.IsGlobalNamespace)
            return null;
        return symbol.ToDisplayString();
    }
}
