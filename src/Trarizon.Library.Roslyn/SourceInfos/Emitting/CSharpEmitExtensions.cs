using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Trarizon.Library.Roslyn.Emitting;

namespace Trarizon.Library.Roslyn.SourceInfos.Emitting;

public static class CSharpEmitExtensions
{
    public static EmitterIndentScope EmitCSharpTypeHierarchy(this IndentedTextWriter writer, TypeHierarchyInfo? type, bool partial,
        Action<IndentedTextWriter, TypeHierarchyInfo>? customLeafTypeEmitter = null)
    {
        if (type is null)
            return default;

        var cur = type.Parent;
        var stack = new Stack<TypeHierarchyInfo>();
        while (cur is not null) {
            stack.Push(cur);
            cur = cur.Parent;
        }
        var defer = writer.EnterIndentTrackingScope();

        if (type.Namespace is not null) {
            defer.Writer.WriteLine($"namespace {type.Namespace}");
            defer.Writer.WriteLine("{");
            defer.Indent("}");
        }

        string partialKeyword = partial ? "partial " : string.Empty;
        foreach (var t in stack) {
            defer.Writer.WriteLine($"{partialKeyword}{t.Keywords} {t.Name}");
            defer.WriteBracketAndIndent('{');
        }

        if(customLeafTypeEmitter is not null)
            customLeafTypeEmitter(defer.Writer, type);
        else
            defer.Writer.WriteLine($"{partialKeyword}{type.Keywords} {type.Name}");

        defer.WriteBracketAndIndent('{');
        return defer;
    }

    public static PreprocessorConditionalScope EmitCSharpPreprocessorConditional(this IndentedTextWriter writer, string ifCondition)
        => new PreprocessorConditionalScope(writer, ifCondition);

    public readonly struct PreprocessorConditionalScope : IDisposable
    {
        private readonly IndentedTextWriter _writer;

        public PreprocessorConditionalScope(IndentedTextWriter writer, string text)
        {
            _writer = writer;
            _writer.WriteLineNoTabs($"#if {text}");
        }

        public void EmitElif(string conditionText)
        {
            _writer.WriteLineNoTabs($"#elif {conditionText}");
        }

        public void EmitElse()
        {
            _writer.WriteLineNoTabs("#else");
        }

        public void Dispose()
        {
            _writer.WriteLineNoTabs("#endif");
        }
    }
}
