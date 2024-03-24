using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trarizon.Library.SourceGenerator.Toolkit;
public struct DiagnosticContext<T>
{
    private readonly T _value;
    private List<Diagnostic>? _diagnostics;

    public readonly IEnumerable<Diagnostic> Diagnostics => _diagnostics ?? Enumerable.Empty<Diagnostic>();

    public readonly T Value => _value;

    private DiagnosticContext(T value, List<Diagnostic>? diagnostics)
    {
        _value = value;
        _diagnostics = diagnostics;
    }

    public DiagnosticContext(T value)
    {
        _value = value;
    }

    private void AddDiagnostic(Diagnostic diagnostic)
        => (_diagnostics ??= []).Add(diagnostic);

    private void AddDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        if (_diagnostics is null)
            _diagnostics = new List<Diagnostic>(diagnostics);
        else
            _diagnostics.AddRange(diagnostics);
    }

    public DiagnosticContext<T> Validate(Func<T, Diagnostic?> validation)
    {
        Diagnostic? diag;
        try {
            diag = validation(_value);
        } catch (Exception) {
            return this;
        }
        if (diag is not null)
            AddDiagnostic(diag);
        return this;
    }

    public DiagnosticContext<T> Validate(Func<T, IEnumerable<Diagnostic>> validation)
    {
        IEnumerable<Diagnostic> diags;
        try {
            diags = validation(_value);
        } catch (Exception) {
            return this;
        }
        AddDiagnostics(diags);
        return this;
    }

    public readonly DiagnosticContext<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        return new(selector(_value), _diagnostics);
    }
}
