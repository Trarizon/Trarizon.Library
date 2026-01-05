using System;
using System.CodeDom.Compiler;

namespace Trarizon.Library.Roslyn.Emitting;

public static class IndentedTextWriterExtensions
{
    private static readonly string[] _newlineSeperators = ["\n", "\r\n"];

    public static void WriteMultipleLines(this IndentedTextWriter writer, string text)
    {
        foreach (var line in text.Split(_newlineSeperators, StringSplitOptions.None)) {
            writer.WriteLine(line);
        }
    }

    public static EmitterIndentScope EnterBracketIndentScope(this IndentedTextWriter writer, char leftBracket, bool dedentNewLine = true)
    {
        writer.WriteLine(leftBracket);
        writer.Indent++;
        return new EmitterIndentScope(writer, Utils.GetRightBracket(leftBracket)?.ToString() ?? "");
    }

    public static EmitterIndentScope EnterIndentScope(this IndentedTextWriter writer, string? beforeIndentText = null, string? afterDedentText = null)
    {
        if (beforeIndentText is not null)
            writer.WriteLine(beforeIndentText);
        writer.Indent++;
        return new EmitterIndentScope(writer, afterDedentText ?? "");
    }

    public static EmitterIndentTrackingScope EnterIndentTrackingScope(this IndentedTextWriter writer)
    {
        return new EmitterIndentTrackingScope(writer);
    }
}
