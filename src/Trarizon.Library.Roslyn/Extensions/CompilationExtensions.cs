using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Roslyn.SourceInfos;

namespace Trarizon.Library.Roslyn.Extensions;

public static class CompilationExtensions
{
    public static bool TryGetTypeSymbol(this Compilation compilation, TypeNameInfo typeNameInfo, [MaybeNullWhen(false)] out INamedTypeSymbol typeSymbol)
    {
        typeSymbol = compilation.GetTypeByMetadataName(typeNameInfo.MetadataName);
        return typeSymbol is not null;
    }

    public static bool TryGetTypeByMetadataName(this Compilation compilation, string fullyQualifiedMetadataName, [MaybeNullWhen(false)] out INamedTypeSymbol typeSymbol)
    {
        typeSymbol = compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
        return typeSymbol is not null;
    }
}
