using Microsoft.CodeAnalysis;
using Trarizon.Library.Roslyn.CSharp;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration.Internal;

internal static class Utilities
{
    public static string ToFileNameString(this ISymbol symbol)
        => CodeHelpers.ToFileNameString(symbol.ToDisplayString());
}
