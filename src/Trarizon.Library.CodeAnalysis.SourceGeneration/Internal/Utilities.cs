using Microsoft.CodeAnalysis;
using Trarizon.Library.Roslyn.Emitting;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration.Internal;

internal static class Utilities
{
    public static string ToFileNameString(this ISymbol symbol)
        => EmittingHelpers.ToFileNameString(symbol.ToDisplayString());
}
