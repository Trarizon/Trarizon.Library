using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.GeneratorToolkit;
public readonly partial struct FilterResult
{
    public static FilterResult Create(Diagnostic diagnostic) => Unsafe.As<Diagnostic, FilterResult>(ref diagnostic);

    public static FilterResult<T> Create<T>(T value) => new(new(value));
    public static FilterResult<T> Create<T>(Diagnostic diagnostic) => new(new(diagnostic));
    public static FilterResult<T> Create<T>(IEnumerable<Diagnostic> diagnostics) => new(new(diagnostics));

    public static FilterResult Create(IEnumerable<Diagnostic> diagnostics) => Unsafe.As<IEnumerable<Diagnostic>, FilterResult>(ref diagnostics);
}

partial struct FilterResult
{
    private readonly object? _diagnostic;
    internal readonly Diagnostic? Diagnostic => _diagnostic as Diagnostic;
    internal readonly IEnumerable<Diagnostic>? Diagnostics => _diagnostic as IEnumerable<Diagnostic>;
}

public readonly struct FilterResult<T>
{
    private readonly Result<T, object> _result;

    internal readonly T Value => _result.Value;
    internal readonly Diagnostic? Diagnostic => _result.GetErrorOrDefault() as Diagnostic;
    internal readonly IEnumerable<Diagnostic>? Diagnostics => _result.GetErrorOrDefault() as IEnumerable<Diagnostic>;

    internal FilterResult(Result<T, object> result)
        => _result = result;
}