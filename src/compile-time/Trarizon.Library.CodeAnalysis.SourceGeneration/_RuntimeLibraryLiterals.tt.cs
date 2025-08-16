using Microsoft.CodeAnalysis;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
/// <summary>
/// TryGetXXX, get type symbol of runtime lib types
/// </summary>
internal static class CompilationGetTypeSymbols
{
    public static bool TryGetSingletonAttribute(this Compilation compilation, out INamedTypeSymbol singletonAttributeSymbol)
    {
        singletonAttributeSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.CodeAnalysis.Generation.SingletonAttribute")!;
        return singletonAttributeSymbol is not null;
    }

    public static bool TryGetExternalSealedAttribute(this Compilation compilation, out INamedTypeSymbol externalSealedAttributeSymbol)
    {
        externalSealedAttributeSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.CodeAnalysis.Diagnostics.ExternalSealedAttribute")!;
        return externalSealedAttributeSymbol is not null;
    }

    public static bool TryGetFriendAccessAttribute(this Compilation compilation, out INamedTypeSymbol friendAccessAttributeSymbol)
    {
        friendAccessAttributeSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.CodeAnalysis.Diagnostics.FriendAccessAttribute")!;
        return friendAccessAttributeSymbol is not null;
    }

}

internal static class RuntimeTypeMetadataNames
{
    public const string SingletonAttribute = "Trarizon.Library.CodeAnalysis.Generation.SingletonAttribute";
}

