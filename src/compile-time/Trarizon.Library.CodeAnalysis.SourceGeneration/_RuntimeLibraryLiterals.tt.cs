using Microsoft.CodeAnalysis;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
/// <summary>
/// TryGetXXX, get type symbol of runtime lib types
/// </summary>
internal static class CompilationGetTypeSymbols
{
    public static bool TryGetSingletonAttribute(this Compilation compilation, out INamedTypeSymbol SingletonAttributeSymbol)
    {
        SingletonAttributeSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.CodeAnalysis.Generation.SingletonAttribute")!;
        return SingletonAttributeSymbol is not null;
    }

    public static bool TryGetExternalSealedAttribute(this Compilation compilation, out INamedTypeSymbol ExternalSealedAttributeSymbol)
    {
        ExternalSealedAttributeSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.CodeAnalysis.Diagnostics.ExternalSealedAttribute")!;
        return ExternalSealedAttributeSymbol is not null;
    }

    public static bool TryGetFriendAccessAttribute(this Compilation compilation, out INamedTypeSymbol FriendAccessAttributeSymbol)
    {
        FriendAccessAttributeSymbol = compilation.GetTypeByMetadataName("Trarizon.Library.CodeAnalysis.Diagnostics.FriendAccessAttribute")!;
        return FriendAccessAttributeSymbol is not null;
    }

}

internal static class RuntimeTypeMetadataNames
{
    public const string SingletonAttribute = "Trarizon.Library.CodeAnalysis.Generation.SingletonAttribute";
}

