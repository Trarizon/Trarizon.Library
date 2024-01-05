﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.GeneratorToolkit.Utilities;
using Traw = Trarizon.Library.Wrappers;

namespace Trarizon.Library.GeneratorToolkit;
public sealed class Filter<TContext>
{
    private Traw.Optional<TContext> _context;
    private List<Diagnostic>? _diagnostics;

    public TContext? Context => _context.GetValueOrDefault();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics ?? [];

    [MemberNotNullWhen(false, nameof(Context))]
    public bool HasDiagnostic => _diagnostics?.Count > 0;

    private Filter(Traw.Optional<TContext> context, List<Diagnostic>? diagnostics)
    {
        _context = context;
        _diagnostics = diagnostics;
    }

    public Filter(TContext context) : this(context, null)
    { }

    public static Filter<TContext> Create<TSource>(in TSource source, Func<TSource, FilterResult<TContext>> selector)
    {
        var result = selector(source);
        if (result.Error)
            return new(default, [result.Diagnostic]);
        else
            return new(result.Value, null);
    }

    public Filter<TContext> Predicate(Func<TContext, FilterResult> predicate)
    {
        if (_context is (true, var val)) {
            var result = predicate(val);
            if (result.Diagnostic != null) {
                (_diagnostics ??= []).Add(result.Diagnostic);
                if (result.WillClose)
                    _context = default;
            }
        }
        return this;
    }

    public Filter<TResult> Select<TResult>(Func<TContext, FilterResult<TResult>> selector)
    {
        if (_context is (true, var context)) {
            var result = selector(context);
            if (result.Error)
                (_diagnostics ??= []).Add(result.Diagnostic);
            else
                return new(result.Value, _diagnostics);
        }
        return new(default, _diagnostics);
    }
}
