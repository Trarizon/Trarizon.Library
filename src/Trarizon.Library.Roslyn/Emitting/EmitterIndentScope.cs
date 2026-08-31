using System.CodeDom.Compiler;

namespace Trarizon.Library.Roslyn.Emitting;

public readonly struct EmitterIndentScope : IDisposable
{
    private static readonly object _noWriteSentinel = new();

    private readonly IndentedTextWriter _writer;
    private readonly object? _suffixes;

    internal EmitterIndentScope(IndentedTextWriter writer, string?[] suffixes)
    {
        _writer = writer;
        _suffixes = suffixes;
    }

    internal EmitterIndentScope(IndentedTextWriter writer, string? suffix)
    {
        _writer = writer;
        _suffixes = suffix;
    }

    internal EmitterIndentScope(IndentedTextWriter writer)
    {
        _writer = writer;
        _suffixes = _noWriteSentinel;
    }

    public void Dispose()
    {
        if (_writer is null)
            return;

        // null - no dedent and no write
        if (_suffixes is null)
        {
            return;
        }

        // non-Array:
        // _sentinel - dedent but no write
        if (_suffixes == _noWriteSentinel)
        {
            _writer.Indent--;
            return;
        }

        // non-Array:
        // string - dedent and write line
        if (_suffixes is string str)
        {
            _writer.Indent--;
            _writer.WriteLine(str);
            return;
        }

        // Array:
        // null: dedent but no write
        // string: dedent and write line
        foreach (var suf in (string?[])_suffixes)
        {
            _writer.Indent--;
            if (suf is not null)
                _writer.WriteLine(suf);
        }
    }
}
