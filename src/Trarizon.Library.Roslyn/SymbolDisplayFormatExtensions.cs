using Microsoft.CodeAnalysis;

namespace Trarizon.Library.Roslyn;

public static class SymbolDisplayFormatExtensions
{
    private static SymbolDisplayFormat? _defaultWithGenericSymbolDisplayFormat;
    private static SymbolDisplayFormat? _fullyQualifiedWithNullableAnnotationFormat;

    extension(SymbolDisplayFormat)
    {
        /// <remarks>
        /// eg: <c>List&lt;T></c> as <c>System.Collections.Generic.List</c>
        /// </remarks>
        public static SymbolDisplayFormat DefaultWithoutGenerics => _defaultWithGenericSymbolDisplayFormat ??=
             SymbolDisplayFormat.CSharpErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);

        public static SymbolDisplayFormat FullyQualifiedWithNullableAnnotation => _fullyQualifiedWithNullableAnnotationFormat ??=
            SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    }
}
