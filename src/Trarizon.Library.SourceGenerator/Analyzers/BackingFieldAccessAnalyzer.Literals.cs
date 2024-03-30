﻿using Microsoft.CodeAnalysis;

namespace Trarizon.Library.SourceGenerator.Analyzers;
partial class BackingFieldAccessAnalyzer
{
    public const string Literal_Attribute_TypeName = $"{Constants.Namespace_CodeAnalysis}.MemberAccess.BackingFieldAccessAttribute";
    public const int Literal_Attribute_AccessableMembers_ConstructorIndex = 0;

    #region Diagnostics

    public static readonly DiagnosticDescriptor Diagnostic_BackingFieldShouldBePrivate = new(
        $"TRA{Constants.BackingFieldAccessAnalyzer_Id}0001",
        nameof(Diagnostic_BackingFieldShouldBePrivate),
        "Backing field should be private",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor Diagnostic_BackingFieldCannotBeAccessed = new(
        $"TRA{Constants.BackingFieldAccessAnalyzer_Id}0002",
        nameof(Diagnostic_BackingFieldCannotBeAccessed),
        "Cannot access a backing field here",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor Diagnostic_TypeDoesnotContainsMember_0MemberName = new(
        $"TRA{Constants.BackingFieldAccessAnalyzer_Id}0003",
        nameof(Diagnostic_TypeDoesnotContainsMember_0MemberName),
        "Cannot find member {0} in type",
        Constants.Category,
        DiagnosticSeverity.Error,
        true);

    #endregion
}
