using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Trarizon.Library.GeneratorToolkit.Factories;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class IndentedTextWriterExtensions
{
    public static DeferIndentBack WriteBraceIndent(this IndentedTextWriter writer)
    {
        writer.WriteLine('{');
        writer.Indent++;
        return new DeferIndentBack(writer, "}");
    }

    public static DeferIndentBack WriteLineWithBrace(this IndentedTextWriter writer, string? text)
    {
        if (text is not null)
            writer.WriteLine(text);
        return writer.WriteBraceIndent();
    }

    public static DeferIndentBack WriteLineWithIndent(this IndentedTextWriter writer, string? text, string? beforeIndent = null, string? afterIndent = null)
    {
        if (text is not null)
            writer.WriteLine(text);
        if (beforeIndent is not null)
            writer.WriteLine(beforeIndent);
        writer.Indent++;
        return new DeferIndentBack(writer, afterIndent);
    }

    public static void WriteMultipleLines(this IndentedTextWriter writer, string text)
    {
        foreach (var line in text.Split('\n')) {
            writer.WriteLine(line);
        }
    }

    public static DeferMultiIndentBackHelper EmitContainingTypesAndNamespace(this IndentedTextWriter writer, ISymbol symbol, SyntaxNode syntax)
    {
        var defer = new DeferMultiIndentBackHelper(writer);
        var ns = symbol.ContainingNamespace.ToNonGlobalDisplayString();
        if (ns is not null) {
            writer.WriteLine($"namespace {ns}");
            defer.IndentWithBrace();
        }

        foreach (var containgType in syntax.Ancestors().OfTypeWhile<TypeDeclarationSyntax>().Reverse()) {
            writer.WriteLine(CodeFactory.ClonePartialDeclaration(containgType));
            defer.IndentWithBrace();
        }
        return defer;
    }


    public ref struct DeferIndentBack(IndentedTextWriter writer, string? lineAfterBack)
    {
        public readonly void Dispose()
        {
            writer.Indent--;
            if (lineAfterBack is not null)
                writer.WriteLine(lineAfterBack);
        }
    }

    public ref struct DeferMultiIndentBackHelper(IndentedTextWriter writer)
    {
        private Stack<string?>? _suffixes;

        public void IndentWithBrace()
        {
            writer.WriteLine('{');
            writer.Indent++;
            (_suffixes ??= []).Push("}");
        }

        public void Indent(string? before, string? after)
        {
            if (before is not null)
                writer.WriteLine(before);
            writer.Indent++;
            (_suffixes ??= []).Push(after);
        }

        public readonly void Dispose()
        {
            if (_suffixes is null)
                return;

            foreach (var after in _suffixes) {
                writer.Indent--;
                if (after is not null)
                    writer.WriteLine(after);
            }
            _suffixes.Clear();
        }
    }
}
