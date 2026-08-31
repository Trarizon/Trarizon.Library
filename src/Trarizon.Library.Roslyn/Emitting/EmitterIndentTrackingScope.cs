using System.CodeDom.Compiler;

namespace Trarizon.Library.Roslyn.Emitting;

public readonly struct EmitterIndentTrackingScope(IndentedTextWriter writer) : IDisposable
{
    private readonly Stack<string> _suffixes = new();

    public IndentedTextWriter Writer => writer;

    public void WriteBracketAndIndent(char leftBracket)
    {
        writer.WriteLine(leftBracket);
        Indent(Utils.GetRightBracket(leftBracket)?.ToString() ?? "");
    }

    public void Indent(string suffix = "")
    {
        writer.Indent++;
        _suffixes.Push(suffix);
    }

    public EmitterIndentScope ToDeferDedentsAndClear()
    {
        var suffixes = _suffixes.ToArray();
        _suffixes.Clear();
        return new EmitterIndentScope(writer, suffixes);
    }

    public readonly void Dispose()
    {
        foreach (var suf in _suffixes)
        {
            writer.Indent--;
            if (suf is not null)
                writer.WriteLine(suf);
        }
        _suffixes.Clear();
    }
}
