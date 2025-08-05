using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Trarizon.Library.Roslyn.SourceInfos.Emitting;
public static class IndentedTextWriterExtensions
{
    private static readonly string[] _newlineSeperators = ["\n", "\r\n"];

    public static DeferDedents EnterBracketDedentScope(this IndentedTextWriter writer, char leftBracket, bool dedentNewLine = true)
    {
        writer.WriteLine(leftBracket);
        writer.Indent++;
        return new DeferDedents(writer, GetRightBracket(leftBracket)?.ToString());
    }

    public static void WriteMultipleLines(this IndentedTextWriter writer, string text)
    {
        foreach (var line in text.Split(_newlineSeperators, StringSplitOptions.None)) {
            writer.WriteLine(line);
        }
    }

    public static IndentTrackingScope EnterIndentTrackingScope(this IndentedTextWriter writer)
    {
        return new IndentTrackingScope(writer);
    }

    private static char? GetRightBracket(char left) => left switch
    {
        '{' => '}',
        '[' => ']',
        '(' => ')',
        '<' => '>',
        _ => null,
    };

    public readonly struct DeferDedents : IDisposable
    {
        private readonly IndentedTextWriter _writer;
        internal readonly object? _suffixes;

        internal DeferDedents(IndentedTextWriter writer, IEnumerable<string?> suffixes)
        {
            _writer = writer;
            _suffixes = suffixes;
        }

        internal DeferDedents(IndentedTextWriter writer, string? suffix)
        {
            _writer = writer;
            _suffixes = suffix;
        }

        public void Dispose()
        {
            if (_writer is null)
                return;

            if (_suffixes is null) {
                _writer.Indent--;
                return;
            }

            if (_suffixes is string str) {
                _writer.Indent--;
                _writer.WriteLine(str);
                return;
            }

            foreach (var suf in (IEnumerable<string?>)_suffixes) {
                _writer.Indent--;
                if (suf is not null)
                    _writer.WriteLine(suf);
            }
        }
    }

    public readonly struct IndentTrackingScope(
        IndentedTextWriter writer)
        : IDisposable
    {
        private readonly Stack<string?> _suffixes = new();

        public IndentedTextWriter Writer => writer;

        public void WriteBracketAndIndent(char leftBracket)
        {
            writer.WriteLine(leftBracket);
            writer.Indent++;
            _suffixes.Push(GetRightBracket(leftBracket)?.ToString());
        }

        public void Append(Func<IndentedTextWriter, DeferDedents> func)
        {
            var suff = func(writer)._suffixes;
            if (suff is null)
                return;
            if (suff is string str) {
                _suffixes.Push(str);
                return;
            }
            foreach (var sfx in (IEnumerable<string?>)suff) {
                _suffixes.Push(sfx);
            }
        }

        public DeferDedents ToDeferDedents() => new DeferDedents(writer, _suffixes);

        public readonly void Dispose()
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
