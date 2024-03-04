using Microsoft.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit;
public static class CustomSymbolDisplayFormats
{
    public static readonly SymbolDisplayFormat DefaultWithoutGeneric = SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);
}
