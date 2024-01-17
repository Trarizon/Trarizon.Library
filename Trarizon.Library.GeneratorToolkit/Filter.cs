using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.GeneratorToolkit;
public partial struct Filter
{
    public static Filter Success => default;
    public static Filter Create(Diagnostic diagnostic) => Unsafe.As<Diagnostic, Filter>(ref diagnostic);
    public static Filter Create(IEnumerable<Diagnostic> diagnostics) => Unsafe.As<IEnumerable<Diagnostic>, Filter>(ref diagnostics);

    public static Filter<TContext> Create<TContext>(in TContext context) where TContext : notnull => new(context, null);
    public static Filter<TContext> Create<TContext>(Diagnostic diagnostic) => new(default, [diagnostic]);
    public static Filter<TContext> Create<TContext>(List<Diagnostic> diagnostic) => new(default, diagnostic);
    public static Filter<TContext> Create<TContext>(IEnumerable<Diagnostic> diagnostics) => new(default, [.. diagnostics]);
}

public readonly partial struct Filter
{
    internal readonly object? _diagnostic;
}

public struct Filter<TContext>
{
    private Optional<TContext> _context;
    private List<Diagnostic>? _diagnostics;

    internal Filter(Optional<TContext> context, List<Diagnostic>? diagnostics)
    {
        _context = context;
        _diagnostics = diagnostics;
    }

    public static implicit operator Filter(Filter<TContext> filter) => filter._diagnostics switch {
        null or [] => Filter.Success,
        [var diag] => Filter.Create(diag),
        _ => Filter.Create(filter._diagnostics),
    };

    public readonly void OnFinal(Action<TContext> onNotClosed, Action<Diagnostic> onHasDiagnostic)
    {
        if (_context.HasValue) {
            onNotClosed.Invoke(_context.Value);
        }

        if (_diagnostics is not null) {
            foreach (var diagnostic in _diagnostics) {
                onHasDiagnostic.Invoke(diagnostic);
            }
        }
    }

    public readonly Filter<TContext> Do(Action action)
    {
        action();
        return this;
    }

    public Filter<TContext> CloseIfHasDiagnostic()
    {
        if (_diagnostics?.Count > 0)
            _context = default;
        return this;
    }

    public Filter<TContext> Predicate(Func<TContext, Filter> predicate)
    {
        if (!_context.HasValue)
            return this;

        HandlePredicateResultInternal(predicate.Invoke(_context.Value));

        return this;
    }

    public Filter<TContext> PredicateMany<T>(Func<TContext, IEnumerable<T>> selector, Func<T, Filter> predicate)
    {
        if (!_context.HasValue)
            return this;

        foreach (var value in selector.Invoke(_context.Value)) {
            HandlePredicateResultInternal(predicate.Invoke(value));
        }

        return this;
    }

    private void HandlePredicateResultInternal(Filter filter)
    {
        switch (filter._diagnostic) {
            case Diagnostic diagnostic:
                (_diagnostics ??= []).Add(diagnostic);
                break;
            case List<Diagnostic> diagnosticList:
                if (_diagnostics is null or [])
                    _diagnostics = diagnosticList;
                else
                    (_diagnostics ??= []).AddRange(diagnosticList);
                break;
            case IEnumerable<Diagnostic> diagnostics:
                (_diagnostics ??= []).AddRange(diagnostics);
                break;
        }
    }

    public Filter<TResult> Select<TResult>(Func<TContext, Filter<TResult>> selector) where TResult : notnull
    {
        if (_context.HasValue)
            return new(default, _diagnostics);

        var result = selector.Invoke(_context.Value);
        if (result._diagnostics?.Count > 0) {
            if (_diagnostics is null or [])
                _diagnostics = result._diagnostics;
            else
                _diagnostics.AddRange(_diagnostics);
        }

        return new(result._context, _diagnostics);
    }
}
