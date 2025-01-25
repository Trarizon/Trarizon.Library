using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;
using System.Text;
using Trarizon.Library.GeneratorToolkit.ContextModelExtensions;
using Trarizon.Library.GeneratorToolkit.CoreLib.Collections;

namespace Trarizon.Library.GeneratorToolkit.CSharp;
public static class IndentedTextWriterExtensions
{
    private static readonly string[] _lineSplitSeperator = ["\r\n", "\n"];

    /// <summary>
    /// Write a bracket and indent, auto indent back
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="leftBracket"></param>
    public static DeferDedent WriteBracketIndentScope(this IndentedTextWriter writer, char leftBracket)
    {
        writer.WriteLine(leftBracket);
        writer.Indent++;
        return new DeferDedent(writer, GetRightBracket(leftBracket));
    }

    public static void WriteMultipleLines(this IndentedTextWriter writer, string text)
    {
        foreach (var line in text.Split(_lineSplitSeperator, StringSplitOptions.None)) {
            writer.WriteLine(line);
        }
    }

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

    public static DeferMultiDedent EmitContainingTypesAndNamespaces(this IndentedTextWriter writer, ISymbol symbol, SyntaxNode syntax)
    {
        var defer = new DeferMultiDedent(writer);
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
        writer.WriteLine($"#pragma warning {(enable ? "restore" : "disable")} {errorCodes}");
    }

    private static string? GetRightBracket(char leftBracket) => leftBracket switch
    {
        '(' => ")",
        '{' => "}",
        '[' => "]",
        '<' => ">",
        _ => null
    };

    public readonly ref struct DeferDedent(IndentedTextWriter writer, string? dedentedText)
    {
        public void Dispose()
        {
            writer.Indent--;
            if (dedentedText is not null)
                writer.WriteLine(dedentedText);
        }
    }

    public readonly ref struct DeferMultiDedent(IndentedTextWriter writer)
    {
        private readonly Stack<string?> _suffixes = new();

        public void WriteBracketAndIndent(char leftBracket)
        {
            writer.WriteLine(leftBracket);
            writer.Indent++;
            _suffixes.Push(GetRightBracket(leftBracket));
        }

        public void Dispose()
        {
            for (int i = _suffixes.Count - 1; i >= 0; i--) {
                var suf = _suffixes.Pop();
                writer.Indent--;
                if (suf is not null)
                    writer.WriteLine(suf);
            }
            _suffixes.Clear();
        }
    }
}
