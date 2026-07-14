using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Roslyn.Extensions;

public static class CompilationExtensions
{
    public static bool TryGetTypeByMetadataName(this Compilation compilation, string fullyQualifiedMetadataName, [MaybeNullWhen(false)] out INamedTypeSymbol typeSymbol)
    {
        typeSymbol = compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
        return typeSymbol is not null;
    }
}
