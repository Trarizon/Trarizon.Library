using System.CodeDom.Compiler;
using Trarizon.Library.Roslyn.Emitting;
using Trarizon.Library.Roslyn.Pipeline;

namespace Trarizon.Library.Roslyn.CSharp;

public static class EmitExtensions
{
    public static EmitterIndentScope EmitCSharpPartialTypeHierarchy(this IndentedTextWriter writer, TypeHierarchyInfo type)
    {
        if (type == default)
            return default;

        var defer = writer.EnterIndentTrackingScope();

        if (type.Namespace is not null)
        {
            defer.Writer.WriteLine(Fmt($"namespace {type.Namespace}"));
            defer.WriteBracketAndIndent('{');
        }

        foreach (var t in type.Types.Span)
        {
            defer.Writer.WriteLine(Fmt($"partial {t.Keywords} {t.Name}"));
            defer.WriteBracketAndIndent('{');
        }

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
            _writer.WriteLineNoTabs(Fmt(null, stackalloc char[4 + text.Length], $"#if {text}"));
        }

        public void EmitElif(string conditionText)
        {
            _writer.WriteLineNoTabs(Fmt(null, stackalloc char[6 + conditionText.Length], $"#elif {conditionText}"));
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
