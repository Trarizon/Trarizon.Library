using Microsoft.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.More;
public static class MoreSymbolDisplayFormats
{
    /// <remarks>
    /// eg: <c>List&lt;T></c> as <c>System.Collections.Generic.List</c>
    /// </remarks>
    public static readonly SymbolDisplayFormat DefaultWithoutGeneric = SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);
}
