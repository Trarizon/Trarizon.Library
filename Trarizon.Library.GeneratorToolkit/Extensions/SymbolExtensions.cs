using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    /// <summary>
    /// Get the full qualified name of a symbol.
    /// eg: <c>global::System.Collections.Generic.List&lt;int&gt;</c> for
    /// <c>List&lt;int&gt;</c>
    /// </summary>
    /// <param name="withPrefixGlobalKeyword">
    /// if <see langword="true"/>, the return string will contains prefix <c>global::</c>
    /// </param>
    /// <returns>
    /// The full qualified name of a symbol, 
    /// <see langword="null"/> if <paramref name="symbol"/> is the top 
    /// namespace (the namespace of any definited namespace or type)
    /// </returns>
    public static string? GetFullName(this ISymbol symbol)
    {
        Stack<string> stack = new(8);
        int fullNameLength = 0;

        // if a namespace or type has no containing symbol, symbol.ContainingSymbol
        // returns a symbol with empty name, which should be omitted
        while (symbol.ContainingSymbol is INamespaceOrTypeSymbol) {
            string symbolName = symbol switch {
                INamedTypeSymbol typeSymbol => typeSymbol.GetNameWithTypeArguments(),
                _ => symbol.Name,
            };
            stack.Push(symbolName);
            fullNameLength += symbolName.Length;
            symbol = symbol.ContainingSymbol;
        }

        if (fullNameLength == 0)
            return null;

        ReadOnlySpan<char> prefix = "global::".AsSpan();

        // str_len + trailing '.' + prefix
        Span<char> fullString = stackalloc char[fullNameLength + stack.Count + prefix.Length];

        prefix.CopyTo(fullString);
        CopyStackItem(stack, fullString[prefix.Length..]);
        // Remove trailing '.'
        return fullString[..^1].CreateString();

        static void CopyStackItem(Stack<string> stack, Span<char> builder)
        {
            int index = 0;
            foreach (var symbolName in stack) {
                symbolName.AsSpan().CopyTo(builder[index..]);
                index += symbolName.Length;
                builder[index++] = '.';
            }
        }
    }

    /// <summary>
    /// Get type name with type arguments.
    /// eg: <c>List&lt;T&gt;</c> for <c>List&lt;T&gt;</c>,
    /// <c>List&lt;int&gt;</c> for <c>List&lt;int&gt;</c>
    /// </summary>
    public static string GetNameWithTypeArguments(this INamedTypeSymbol symbol)
    {
        var typeArgs = symbol.TypeArguments;
        if (typeArgs.Length == 0)
            return symbol.Name;

        // name + <>
        int nameLength = symbol.Name.Length + 2;
        foreach (var arg in typeArgs) {
            // name + ,
            nameLength += arg.Name.Length + 1;
        }

        // Remove trailing ','
        Span<char> buffer = stackalloc char[nameLength - 1];

        int index = 0;
        symbol.Name.AsSpan().CopyTo(buffer);
        index += symbol.Name.Length;
        buffer[index++] = '<';
        foreach (var arg in typeArgs) {
            arg.Name.AsSpan().CopyTo(buffer[index..]);
            index += arg.Name.Length;
            buffer[index++] = ',';
        }
        buffer[index - 1] = '>';
        return buffer.CreateString();
    }
}
