using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.Library.SourceGenerator.Toolkit.Factories;

namespace Trarizon.Library.SourceGenerator.Generators;
partial class SingletonGenerator
{
    public const string L_Attribute_TypeName = $"{Constants.Namespace_CodeTemplating}.SingletonAttribute";
    public const string L_Attribute_InstancePropertyName_PropertyIdentifier = "InstancePropertyName";
    public const string L_Attribute_SingletonProviderName_PropertyIdentifier = "SingletonProviderName";
    public const string L_Attribute_Options_PropertyIdentifier = "Options";


    public const string L_Instance_FieldIdentifier = "__Instance";
    public const string L_Instance_PropertyIdentifier = "Instance";
    public const string L_SingletonProvider_TypeIdentifier = "__SingletonProvider";

    public const string L_GeneratedCodeAttribute_Tool_Argument = $"{Constants.Namespace_SourceGeneraterGenerators}.{nameof(SingletonGenerator)}";

    public static readonly AttributeListSyntax Syntax_GeneratedCodeAttribute_AttributeList =
        SyntaxProvider.GeneratedCodeAttributeListSyntax(
            tool: L_GeneratedCodeAttribute_Tool_Argument,
            version: Constants.Version);

    #region Diagnostics

    public static readonly DiagnosticDescriptor D_SingletonIsClassOnly = new(
        $"TRA{Constants.SingletonGenerator_Id}0001",
        nameof(D_SingletonIsClassOnly),
        "Only class type can be singleton",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonIsSealed = new(
        $"TRA{Constants.SingletonGenerator_Id}0002",
        nameof(D_SingletonIsSealed),
        "Singleton class should be sealed to avoid multiple instances",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonCannotContainsNonPrivateCtor = new(
        $"TRA{Constants.SingletonGenerator_Id}0003",
        nameof(D_SingletonCannotContainsNonPrivateCtor),
        "Do not provide non-private constructor to avoid multiple instances",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonHasOneOrNoneCtor = new(
        $"TRA{Constants.SingletonGenerator_Id}0004",
        nameof(D_SingletonHasOneOrNoneCtor),
        "constructor should have single or no constructor",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonCtorHasNoParameter = new(
        $"TRA{Constants.SingletonGenerator_Id}0005",
        nameof(D_SingletonCtorHasNoParameter),
        "constructor should have no parameter",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    // Analyzer

    public static readonly DiagnosticDescriptor Diagnostic_SingletonCtorIsNotAccessable = new(
        $"TRA{Constants.SingletonGenerator_Id}1001",
        nameof(Diagnostic_SingletonCtorIsNotAccessable),
        "Do not call singleton ctor manually",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    #endregion
}
