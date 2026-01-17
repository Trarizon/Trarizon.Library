using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Trarizon.Library.Functional;

namespace Trarizon.Library.Roslyn.SourceInfos.CSharp;

public static partial class CodeFactory
{
    public static string Literal(string value) => $"\"{value}\"";
    public static string LiteralUtf8(string value) => $"\"{value}\"u8";
    public static string Literal(bool value) => value ? "true" : "false";

    public static string GetMethodDefinationText(IMethodSymbol symbol, MethodDeclarationSyntax syntax, bool ensurePartialModifier = false)
    {
        var syntaxModifiers = syntax.Modifiers;
        if (ensurePartialModifier) {
            if (!syntaxModifiers.Any(SyntaxKind.PartialKeyword)) {
                syntaxModifiers = syntaxModifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            }
        }
        var refModifiers = symbol.ReturnsByRef ? "ref "
            : symbol.ReturnsByRefReadonly ? "ref readonly "
            : "";
        var returnType = symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var explicitInterface = symbol.ExplicitInterfaceImplementations.Length > 0
            ? symbol.ExplicitInterfaceImplementations[0].ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : null;
        var name = $"{syntax.Identifier}{syntax.TypeParameterList}";
        var parameters = symbol.Parameters
            .Zip(syntax.ParameterList.Parameters, static (symbol, syntax) =>
            {
                var modifiers = syntax.Modifiers.ToString();
                var type = symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var name = symbol.Name;
                var defaultValue = Optional.Create<object?>(symbol.HasExplicitDefaultValue, symbol.ExplicitDefaultValue);
                return (modifiers, type, name, defaultValue);
            })
            .ToSequenceEquatableImmutableArray();
        var constraints = symbol.TypeParameters
            .Join(syntax.ConstraintClauses, sym => sym.Name, syn => syn.Name.Identifier.Text, static (symbol, syntax) =>
            {
                return GetConstraintText(symbol);
            })
            .ToSequenceEquatableImmutableArray();

        // Get String
        var prms = parameters.Select(p =>
        {
            var (modifiers, type, name, defaultValue) = p;
            var mod = string.IsNullOrEmpty(modifiers) ? null : $"{modifiers} ";
            var def = defaultValue.HasValue ? $" = {defaultValue.Value}" : null;
            return $"{mod}{type} {name}{def}";
        });
        var intf = explicitInterface is not null ? $"{explicitInterface}." : null;
        return string.Join(" ", [
            .. syntaxModifiers.Select(tk => tk.ToString()),
            $"{refModifiers}{returnType}",
            $"{intf}{name}({string.Join(", ", prms)})",
            .. constraints
            ]);
    }

    public static string? GetConstraintText(ITypeParameterSymbol typeParameter)
    {
        List<string> constraints = [];
        if (typeParameter.HasReferenceTypeConstraint) {
            if (typeParameter.ReferenceTypeConstraintNullableAnnotation is NullableAnnotation.Annotated)
                constraints.Add("class?");
            else
                constraints.Add("class");
        }
        if (typeParameter.HasValueTypeConstraint)
            constraints.Add("struct");
        if (typeParameter.HasNotNullConstraint)
            constraints.Add("notnull");
        if (typeParameter.HasUnmanagedTypeConstraint)
            constraints.Add("unmanaged");
        if (typeParameter.ConstraintTypes.Length > 0)
            constraints.AddRange(typeParameter.ConstraintTypes.Select(t => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        if (typeParameter.HasConstructorConstraint)
            constraints.Add("new()");
        if (typeParameter.AllowsRefLikeType)
            constraints.Add("allows ref struct");
        if (constraints.Count == 0)
            return null;
        return $"where {typeParameter.Name} : {string.Join(", ", constraints)}";
    }

    public static string GetParameterRefKeywords(RefKind refKind, bool trailingWhitespace = false)
    {
        var res = refKind switch
        {
            RefKind.Ref => "ref",
            RefKind.Out => "out",
            RefKind.In => "in",
            RefKind.RefReadOnlyParameter => "ref readonly",
            _ => "in",
        };
        return trailingWhitespace && res != "" ? $"{res} " : res;
    }

    // M(ref a, in b, c)
    //   ^this  ^this
    public static string GetArgumentRefKeywords(RefKind refKind, bool trailingWhitespace = false)
    {
        var res = refKind switch
        {
            RefKind.Ref => "ref",
            RefKind.Out => "out",
            RefKind.In => "in",
            RefKind.RefReadOnlyParameter => "in",
            _ => "",
        };
        return trailingWhitespace && res != "" ? $"{res} " : res;
    }

    public static string GetReturnRefKeywords(RefKind refKind, bool trailingWhitespace = false)
    {
        var res = refKind switch
        {
            RefKind.Ref => "ref",
            RefKind.RefReadOnly => "ref",
            _ => "",
        };
        return trailingWhitespace && res != "" ? $"{res} " : res;
    }

    /// <summary>
    /// <c>#pragma warning disable/restore {err-codes}</c>
    /// </summary>
    public static string PragmaWarningTrivia(bool restore, params string[] errorCodes)
        => $"#pragma warning {(restore ? "restore" : "disable")} {string.Join(", ", errorCodes)}";
}