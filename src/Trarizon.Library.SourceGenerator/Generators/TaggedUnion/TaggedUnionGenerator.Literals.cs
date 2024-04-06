using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Generators;
partial class TaggedUnionGenerator
{
    public const string L_Attribute_TypeName = $"{Literals.CodeTemplating_Namespace}.TaggedUnion.UnionTagAttribute";
    public const string L_VariantAttribute_TypeName = $"{Literals.CodeTemplating_Namespace}.TaggedUnion.TagVariantAttribute";

    public const int L_Attribute_GeneratedTypeName_ConstructorIndex = 0;
    public const int L_VariantAttribute_Types_ConstructorIndex = 0;
    public const string L_VariantAttribute_Identifiers_PropertyIdentifier = "Identifiers";

    public const string L_UnionStruct_TypeIdentifier = "__UnionStruct";
    public const string L_UnionStruct_FieldIdentifier = "__struct";
    public const string L_Tag_PropertyIdentifier = "Tag";
    public const string L_RefObj_FieldIdentifierPrefix = "__refObj";
    public static string L_RefObj_FieldIdentifier(int index) => $"{L_RefObj_FieldIdentifierPrefix}{index}";

    public static readonly DiagnosticDescriptor D_InvalidIdentifier = new(
        $"TRA{Literals.TaggedUnionGenerator_Id}0001",
        nameof(D_InvalidIdentifier),
        "TaggedUnion type identifier is invalid",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_VariantFieldNameRepeat = new(
        $"TRA{Literals.TaggedUnionGenerator_Id}0002",
        nameof(D_InvalidIdentifier),
        "Variant field name repeat",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_ProvideNonVariantZeroField = new(
        $"TRA{Literals.TaggedUnionGenerator_Id}0003",
        nameof(D_ProvideNonVariantZeroField),
        "Provide a zero field with no variant, so there won't be unexcepted actions on call constructor or use default",
        Literals.Category,
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor D_EnumValueRepeat = new(
        $"TRA{Literals.TaggedUnionGenerator_Id}0004",
        nameof(D_EnumValueRepeat),
        "Value of enum field repeated",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_ManagedValueTypeWillBeBoxes = new(
        $"TRA{Literals.TaggedUnionGenerator_Id}0005",
        nameof(D_ManagedValueTypeWillBeBoxes),
        "Managed value types will be boxed",
        Literals.Category,
        DiagnosticSeverity.Info,
        true);
}
