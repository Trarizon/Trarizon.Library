using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Traw = Trarizon.Library.Wrappers;

namespace Trarizon.Library.GeneratorToolkit;
public static class Filter
{
    public static Filter<TResult> Create<TSource, TResult>(in TSource source, Func<TSource, FilterResult<TResult>> selector)
    {
        var result = selector(source);
        if (result.Diagnostic != null)
            return new(default, [result.Diagnostic]);
        else if (result.Diagnostics != null)
            return new(default, [.. result.Diagnostics]);
        else
            return new(result.Value, null);
    }
}

public sealed class Filter<TContext>
{
    private Traw.Optional<TContext> _context;
    private List<Diagnostic>? _diagnostics;

    public TContext? Context => _context.GetValueOrDefault();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics ?? [];

    [MemberNotNullWhen(false, nameof(Context))]
    public bool HasDiagnostic => _diagnostics?.Count > 0;

    internal Filter(Traw.Optional<TContext> context, List<Diagnostic>? diagnostics)
    {
        _context = context;
        _diagnostics = diagnostics;
    }

    public Filter(TContext context) : this(context, null)
    { }

    public Filter<TContext> CloseIfHasDiagnostic()
    {
        if (HasDiagnostic)
            _context = default;
        return this;
    }

    public Filter<TContext> Predicate(Func<TContext, FilterResult> predicate)
    {
        if (_context is (true, var val)) {
            var result = predicate(val);
            if (result.Diagnostic != null)
                (_diagnostics ??= []).Add(result.Diagnostic);
            else if (result.Diagnostics != null)
                (_diagnostics ??= []).AddRange(result.Diagnostics);
        }
        return this;
    }

    public Filter<TResult> Select<TResult>(Func<TContext, FilterResult<TResult>> selector)
    {
        if (_context is (true, var context)) {
            var result = selector(context);
            if (result.Diagnostic != null)
                (_diagnostics ??= []).Add(result.Diagnostic);
            else if (result.Diagnostics != null)
                (_diagnostics ??= []).AddRange(result.Diagnostics);
            else
                return new(result.Value, _diagnostics);
        }
        return new(default, _diagnostics);
    }
}
