using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Linq;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class SymbolExtensions
{
    public static bool HasGeneratedCodeAttribute(this ISymbol symbol)
        => symbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == Literals.GeneratedCodeAttributeFullName);

    public static bool IsMatchMetadataString(this ISymbol symbol, string metadataStringWithArity)
    {
        return IsMatchMetadataString(symbol.ToDisplayString(), metadataStringWithArity);
    }

    internal static bool IsMatchMetadataString(string source, string metadataStringWithArity)
    {
        var displayStr = source;

        Span<char> chars = stackalloc char[displayStr.Length];

        var charsLength = Replace(displayStr.AsSpan(), chars);
        if (chars.Length == 0)
            return displayStr == metadataStringWithArity;

        return chars[..charsLength].SequenceEqual(metadataStringWithArity.AsSpan());

        static int Replace(ReadOnlySpan<char> source, Span<char> dest, bool internalCall = false)
        {
            var index = source.IndexOf('<');
            if (index == -1) {
                if (internalCall) { // copy the rest
                    source.CopyTo(dest);
                    return source.Length;
                };
                return 0;
            }

            source[..index].CopyTo(dest);
            dest[index++] = '`';

            int count = 1;
            for (int i = index; i < source.Length; i++) {
                switch (source[i]) {
                    case '<':
                        i += source[i..].IndexOf('>');
                        break;
                    case ',':
                        count++;
                        break;
                    case '>':
                        var countStr = count.ToString().AsSpan();
                        countStr.CopyTo(dest[index..]);
                        index += countStr.Length;
                        return index + Replace(source[(i + 1)..], dest[index..], true);
                }
            }
            Debug.Assert(false, "Unreachable");
            return default;
        }
    }
}
