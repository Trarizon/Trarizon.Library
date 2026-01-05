using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Trarizon.Library.Roslyn.Emitting;

public readonly struct EmitterIndentTrackingScope(IndentedTextWriter writer) : IDisposable
{
    private readonly Stack<string> _suffixes = new();

    public IndentedTextWriter Writer => writer;

    public void WriteBracketAndIndent(char leftBracket)
    {
        writer.WriteLine(leftBracket);
        writer.Indent++;
        _suffixes.Push(Utils.GetRightBracket(leftBracket)?.ToString() ?? "");
    }

    public void Indent(string suffix = "")
    {
        writer.Indent++;
        _suffixes.Push(suffix);
    }

    public EmitterIndentScope ToDeferDedents() => new EmitterIndentScope(writer, _suffixes);

    public static implicit operator EmitterIndentScope(EmitterIndentTrackingScope scope) => scope.ToDeferDedents();

    public readonly void Dispose()
    {
        for (int i = _suffixes.Count - 1; i >= 0; i--) {
            var suf = _suffixes.Pop();
            writer.Indent--;
            writer.WriteLine(suf);
        }
        _suffixes.Clear();
    }
}
