using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text;
using Trarizon.Library.Collections;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Roslyn.CSharp;
public static class IndentedTextWriterCSharpExtensions
{
    public static void WriteXmlDocLine(this IndentedTextWriter writer, string text, bool noEscape = false)
    {
        if (!noEscape)
            text = Escape(text);
        writer.WriteLine($"/// {text}");

        static string Escape(string text)
        {
            //                                       guessLength
            var sb = new StringBuilder(text.Length + 6);
            foreach (var c in text) {
                if (c is '<')
                    sb.Append("&lt;");
                else if (c is '>')
                    sb.Append("&gt;");
                else if (c is '&')
                    sb.Append("&amp;");
                else if (c is '\n')
                    sb.Append("<br/>");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }

    public static IndentedTextWriterExtensions.DeferMultiDedent EmitContainingTypesAndNamespaces(this IndentedTextWriter writer, ISymbol symbol, SyntaxNode syntax)
    {
        var defer = writer.WriteEnterMultiIndentScope();
        var ns = symbol.ContainingNamespace.ToNonGlobalDisplayString();
        if (ns is not null) {
            writer.WriteLine($"namespace {ns}");
            defer.WriteBracketAndIndent('{');
        }

        foreach (var containgType in syntax.Ancestors().OfTypeWhile<TypeDeclarationSyntax>().Reverse()) {
            writer.WriteLine(CodeFactory.ClonePartialDeclaration(containgType));
            defer.WriteBracketAndIndent('{');
        }
        return defer;
    }

    /// <summary>
    /// Emit <c>#pragma warning disable/restore {err-codes}</c>
    /// </summary>
    public static void EmitPragmaWarningTrivia(this IndentedTextWriter writer, string errorCodes, bool enable)
    {
        writer.WriteLine(CodeFactory.PragmaWarningTrivia(errorCodes, enable));
    }
}
