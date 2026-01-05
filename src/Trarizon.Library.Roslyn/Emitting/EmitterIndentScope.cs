using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Trarizon.Library.Roslyn.Emitting;

public readonly struct EmitterIndentScope : IDisposable
{
    private readonly IndentedTextWriter _writer;
    private readonly object? _suffixes;

    internal EmitterIndentScope(IndentedTextWriter writer, IEnumerable<string?> suffixes)
    {
        _writer = writer;
        _suffixes = suffixes;
    }

    internal EmitterIndentScope(IndentedTextWriter writer, string suffix)
    {
        _writer = writer;
        _suffixes = suffix;
    }

    internal EmitterIndentScope(IndentedTextWriter writer)
    {
        _writer = writer;
        _suffixes = null;
    }

    public void Dispose()
    {
        if (_writer is null)
            return;

        if (_suffixes is null)
            return;

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
