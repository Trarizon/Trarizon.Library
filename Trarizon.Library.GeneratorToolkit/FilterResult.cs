using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.GeneratorToolkit;
public readonly partial struct FilterResult
{
    public static FilterResult Create(Diagnostic diagnostic) => Unsafe.As<Diagnostic, FilterResult>(ref diagnostic);

    public static FilterResult<T> Create<T>(T value) => new(value, default(Diagnostic));
    public static FilterResult<T> Create<T>(Diagnostic diagnostic) => new(default, diagnostic);
    public static FilterResult<T> Create<T>(IEnumerable<Diagnostic> diagnostics) => new(default, diagnostics);

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
    internal readonly T Value;

    private readonly object? _diagnostic;
    internal readonly Diagnostic? Diagnostic => _diagnostic as Diagnostic;
    internal readonly IEnumerable<Diagnostic>? Diagnostics => _diagnostic as IEnumerable<Diagnostic>;

    internal FilterResult(T? value, Diagnostic? diagnostic)
        => (Value, _diagnostic) = (value!, diagnostic);

    internal FilterResult(T? value, IEnumerable<Diagnostic>? diagnostics)
        => (Value, _diagnostic) = (value!, diagnostics);

    public static implicit operator FilterResult<T>(T value) => new(value, default(Diagnostic));
    public static implicit operator FilterResult<T>(Diagnostic diagnostics) => new(default!, diagnostics);
}