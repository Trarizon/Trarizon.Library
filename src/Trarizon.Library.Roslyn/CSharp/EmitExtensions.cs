using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Trarizon.Library.Roslyn.Emitting;
using Trarizon.Library.Roslyn.Pipeline;

namespace Trarizon.Library.Roslyn.CSharp;

public static class EmitExtensions
{
    public static EmitterIndentScope EmitCSharpTypeHierarchy(this IndentedTextWriter writer, TypeHierarchyInfo? type, bool partial)
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
            defer.WriteBracketAndIndent('{');
        }

        string partialKeyword = partial ? "partial " : string.Empty;
        foreach (var t in stack) {
            defer.Writer.WriteLine($"{partialKeyword}{t.Keywords} {t.Name}");
            defer.WriteBracketAndIndent('{');
        }

        defer.WriteBracketAndIndent('{');
        return defer.ToDeferDedentsAndClear();
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
