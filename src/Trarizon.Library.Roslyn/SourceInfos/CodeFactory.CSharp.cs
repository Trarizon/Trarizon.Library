using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Trarizon.Library.Functional;

namespace Trarizon.Library.Roslyn.SourceInfos;
partial class CodeFactory
{
    public static string Literal(string value) => $"\"{value}\"";
    public static string LiteralUtf8(string value) => $"\"{value}\"u8";
    public static string Literal(bool value) => value ? "true" : "false";

    public static string GetMethodNonbodyDeclaration(IMethodSymbol symbol, MethodDeclarationSyntax syntax, bool ensurePartialModifier = false)
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
                List<string> constraints = new();
                if (symbol.HasReferenceTypeConstraint)
                    constraints.Add("class");
                if (symbol.HasValueTypeConstraint)
                    constraints.Add("struct");
                if (symbol.HasNotNullConstraint)
                    constraints.Add("notnull");
                if (symbol.HasUnmanagedTypeConstraint)
                    constraints.Add("unmanaged");
                if (symbol.ConstraintTypes.Length > 0)
                    constraints.AddRange(symbol.ConstraintTypes.Select(t => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                if (symbol.HasConstructorConstraint)
                    constraints.Add("new()");
                if (symbol.AllowsRefLikeType)
                    constraints.Add("allows ref struct");

                return $"where {symbol.Name} : {string.Join(", ", constraints)}";
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

    /// <summary>
    /// <c>#pragma warning disable/restore {err-codes}</c>
    /// </summary>
    public static string PragmaWarningTrivia(bool restore, params string[] errorCodes)
        => $"#pragma warning {(restore ? "restore" : "disable")} {string.Join(", ", errorCodes)}";

    public static string GeneratedCodeAttributeList(string tool, string version)
        => $"[global::{KnownInfos.GeneratedCodeAttribute.TypeFullName}(\"{tool}\", \"{version}\")]";

    public static string InterceptsLocationAttributeList(string filePath, int line, int character)
        => $"[global::{KnownInfos.InterceptsLocationAttribute.TypeFullName}(\"{filePath}\", {line}, {character})]";
}
