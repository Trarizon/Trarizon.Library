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

/*
 internal struct DiagnosticContext<T>
{
    private readonly IEnumerable<T> _values;
    private bool _success;
    private List<Diagnostic> _diagnostics;
*/
/*
    private DiagnosticContext(IEnumerable<T> values, bool success, List<Diagnostic> diagnostics)
    {
        _values = values;
        _success = success;
        _diagnostics = diagnostics;
    }

    public DiagnosticContext(params T[] values) :
        this(values, true, [])
    { }

    public readonly bool IsClosed => !_success;

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

    public IEnumerable<T> Values => _values;
*/
/*
    public DiagnosticContext<T> Validate(Func<T, Diagnostic?> validation)
    {
        foreach (var value in _values) {
            Diagnostic? diag;
            try {
                diag = validation(value);
            } catch (Exception) {
                continue;
            }
            if (diag is not null)
                _diagnostics.Add(diag);
        }
        return this;
    }

    public DiagnosticContext<T> Validate(Func<T, IEnumerable<Diagnostic>> validation)
    {
        foreach (var value in _values) {
            IEnumerable<Diagnostic> diags;
            try {
                diags = validation(value);
            } catch (Exception) {
                continue;
            }
            _diagnostics.AddRange(diags);
        }
        return this;
    }
*/
/*

    public readonly DiagnosticContext<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        return new(_values.Select(selector), _success, _diagnostics);
    }

    public readonly DiagnosticContext<TResult> SelectMany<TResult>(Func<T, IEnumerable<TResult>> selector)
    {
        return new(_values.SelectMany(selector), _success, _diagnostics);
    }
}

 */