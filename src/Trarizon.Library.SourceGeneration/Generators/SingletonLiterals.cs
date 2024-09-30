using Microsoft.CodeAnalysis;
using System;

namespace Trarizon.Library.SourceGeneration.Generators;
internal static class SingletonLiterals
{
    public const string L_Attribute_TypeName = $"{Literals.CodeGeneration_Namespace}.SingletonAttribute";
    public const string L_Attribute_InstancePropertyName_PropertyIdentifier = "InstancePropertyName";
    public const string L_Attribute_SingletonProviderName_PropertyIdentifier = "SingletonProviderName";
    public const string L_Attribute_Options_PropertyIdentifier = "Options";


    public const string L_Instance_FieldIdentifier = "__Instance";
    public const string L_Instance_PropertyIdentifier = "Instance";
    public const string L_SingletonProvider_TypeIdentifier = "__SingletonProvider";

    #region Diagnostics

    public static readonly DiagnosticDescriptor D_SingletonIsClassOnly = new(
        $"TRA{Literals.SingletonGenerator_Id}0001",
        nameof(D_SingletonIsClassOnly),
        "Only class type can be singleton",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonCannotBeAbstractOrStatic = new(
        $"TRA{Literals.SingletonGenerator_Id}0002",
        nameof(D_SingletonCannotBeAbstractOrStatic),
        "Singleton cannot be abstract of static, because they are sealed",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonCannotContainsNonPrivateCtor = new(
        $"TRA{Literals.SingletonGenerator_Id}0003",
        nameof(D_SingletonCannotContainsNonPrivateCtor),
        "Do not provide non-private constructor to avoid multiple instances",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonHasOneOrNoneCtor = new(
        $"TRA{Literals.SingletonGenerator_Id}0004",
        nameof(D_SingletonHasOneOrNoneCtor),
        "constructor should have single or no constructor",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonCtorHasNoParameter = new(
        $"TRA{Literals.SingletonGenerator_Id}0005",
        nameof(D_SingletonCtorHasNoParameter),
        "constructor should have no parameter",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor D_SingletonMemberNameRepeat = new(
        $"TRA{Literals.SingletonGenerator_Id}0006",
        nameof(D_SingletonMemberNameRepeat),
        "Provider and intance cannot have same name",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    // Analyzer

    public static readonly DiagnosticDescriptor Diagnostic_SingletonCtorIsNotAccessable = new(
        $"TRA{Literals.SingletonGenerator_Id}1001",
        nameof(Diagnostic_SingletonCtorIsNotAccessable),
        "Do not call singleton ctor manually",
        Literals.Category,
        DiagnosticSeverity.Error,
        true);

    #endregion

    [Flags]
    public enum SingletonOptions_Mirror
    {
        None = 0,
        /// <summary>
        /// Do not use provider to create instance
        /// </summary>
        /// <remarks>
        /// By default, we create a nested class with a public field as provider <br/>
        /// If this option set, we directly create and assign to instance property, 
        /// in which case when you use this instance, all other static fields in this
        /// type will be initialized.
        /// </remarks>
        NoProvider = 1 << 0,
        /// <summary>
        /// Mark Instance property internal
        /// </summary>
        IsInternalInstance = 1 << 1,
    }
}
