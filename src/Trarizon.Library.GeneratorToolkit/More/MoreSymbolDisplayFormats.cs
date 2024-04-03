using Microsoft.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.More;
public static class MoreSymbolDisplayFormats
{
    public static readonly SymbolDisplayFormat DefaultWithoutGeneric = SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);
}
