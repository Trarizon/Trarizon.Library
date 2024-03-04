using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Trarizon.Library.GeneratorToolkit;
public partial struct Filter
{
    public static Filter Success() => default;
    public static Filter Failed() => Failed(Array.Empty<Diagnostic>());
    public static Filter Failed(Diagnostic diagnostic) => Failed(new[] { diagnostic });
    public static Filter Failed(IEnumerable<Diagnostic> diagnostics) => Unsafe.As<IEnumerable<Diagnostic>, Filter>(ref diagnostics);

    public static Filter<T> Success<T>(in T context) where T : notnull => new(context, null);
    public static Filter<T> Failed<T>(Diagnostic diagnostic) => new(default, [diagnostic]);
    public static Filter<T> Failed<T>(IEnumerable<Diagnostic> diagnostics) => new(default, diagnostics.ToList());
    public static Filter<T> Failed<T>(in T context, IEnumerable<Diagnostic> diagnostics) => new(context, diagnostics.ToList());
}

public readonly partial struct Filter
{
    private readonly IEnumerable<Diagnostic>? _diagnostics;

    [MemberNotNullWhen(false, nameof(Diagnostics))]
    public bool IsSuccess => _diagnostics is not null;

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics!;
}

public struct Filter<T>
{
    private Optional<T> _value;
    private List<Diagnostic>? _diagnostics;

    public readonly T Value => _value.Value;

    [MemberNotNullWhen(false, nameof(Value))]
    public readonly bool IsClosed => !_value.HasValue;

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(_diagnostics))]
    public readonly bool HasError => _diagnostics?.Any(diag => diag.Severity is DiagnosticSeverity.Error) ?? false;

    public readonly IEnumerable<Diagnostic> Diagnostics => _diagnostics ?? [];

    internal Filter(Optional<T> value, List<Diagnostic>? diagnostics)
    {
        _value = value;
        _diagnostics = diagnostics;
    }

    public static implicit operator Filter(Filter<T> filter) => filter._diagnostics switch {
        [..] => Filter.Failed(filter._diagnostics),
        null => Filter.Success(),
    };

    private void TryAddDiagnostic(Filter filter)
    {
        if (filter.IsSuccess)
            return;

        if (_diagnostics is null)
            _diagnostics = new(filter.Diagnostics);
        else
            _diagnostics.AddRange(filter.Diagnostics);
    }

    public readonly Filter<T> ThrowIfCancelled(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return this;
    }

    public Filter<T> Predicate(Func<T, Filter> predicate)
    {
        if (IsClosed)
            return this;

        var result = predicate(Value);
        TryAddDiagnostic(result);
        return this;
    }

    public Filter<T> Predicate(Func<T, Filter> predicate, out bool predicateResult)
    {
        if (IsClosed) {
            predicateResult = false;
            return this;
        }

        var result = predicate(Value);
        TryAddDiagnostic(result);
        predicateResult = result.IsSuccess;
        return this;
    }

    public Filter<T> Predicate(bool precondition, Func<T, Filter> predicate)
    {
        if (!precondition)
            return this;

        return Predicate(predicate);
    }

    public Filter<T> Predicate(bool precondition, Func<T, Filter> predicate, out bool predicateResult)
    {
        if (!precondition) {
            predicateResult = false;
            return this;
        }

        return Predicate(predicate, out predicateResult);
    }

    public Filter<T> PredicateMany<T2>(Func<T, IEnumerable<T2>> selector, Func<T2, Filter> predicate)
    {
        if (IsClosed)
            return this;

        foreach (var value in selector.Invoke(_value.Value)) {
            var result = predicate(value);
            TryAddDiagnostic(result);
        }

        return this;
    }

    public Filter<TResult> Select<TResult>(Func<T, Filter<TResult>> selector) where TResult : notnull
    {
        if (IsClosed)
            return new(default, _diagnostics);

        var result = selector.Invoke(_value.Value);
        if (result.HasError)
            TryAddDiagnostic(result);
        else
            return new(result.Value, _diagnostics);

        return new(default, _diagnostics);
    }
}
