using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace Trarizon.Library.Roslyn.Extensions;
public static class IndentedTextWriterExtensions
{
    private static readonly string[] _newlineSeperators = ["\n", "\r\n"];

    /// <summary>
    /// Write a bracket and indent, auto indent back
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="leftBracket"></param>
    public static DeferDedent WriteEnterBracketIndentScope(this IndentedTextWriter writer, char leftBracket)
    {
        writer.WriteLine(leftBracket);
        writer.Indent++;
        return new DeferDedent(writer, GetRightBracket(leftBracket).ToString());
    }

    public static void WriteMultipleLines(this IndentedTextWriter writer, string text)
    {
        foreach (var line in text.Split(_newlineSeperators, StringSplitOptions.None)) {
            writer.WriteLine(line);
        }
    }

    public static DeferMultiDedent WriteEnterMultiIndentScope(this IndentedTextWriter writer)
    {
        return new DeferMultiDedent(writer);
    }

    private static char GetRightBracket(char left) => left switch
    {
        '(' => ')',
        '{' => '}',
        '[' => ']',
        '<' => '>',
        _ => left,
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
            _suffixes.Push(GetRightBracket(leftBracket).ToString());
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
