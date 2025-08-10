using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Trarizon.Library.Functional;
using Trarizon.Library.Roslyn.Collections;

namespace Trarizon.Library.Roslyn.Diagnostics;
public static class DiagnosticResult
{
    public static DiagnosticResult<T> Success<T>(T result) => result;
    public static DiagnosticResult<T> Success<T>(T result, SequenceEquatableImmutableArray<DiagnosticData> diagnosticDatas) => new(result, diagnosticDatas);
}

public readonly struct DiagnosticResult<T>
{
    private readonly Optional<T> _result;
    private readonly SequenceEquatableImmutableArray<DiagnosticData> _diags;

    public bool IsSuccess => _result.HasValue;
    public bool IsError => _result.HasValue;
    public Optional<T> Result => _result;
    public T Value => _result.Value;
    public SequenceEquatableImmutableArray<DiagnosticData> Diagnostics => _diags.Array.IsDefault ? new([]) : _diags;

    public DiagnosticResult(Optional<T> result, SequenceEquatableImmutableArray<DiagnosticData> diags)
    {
        _result = result;
        _diags = diags;
    }

    public DiagnosticResult(T result) : this(result, default) { }
    public DiagnosticResult(Optional<T> result) : this(result, default) { }
    public DiagnosticResult(SequenceEquatableImmutableArray<DiagnosticData> diagnostics): this(default, diagnostics) { }
    public DiagnosticResult(DiagnosticData diagnostic) : this(default, new([diagnostic])) { }

    public static implicit operator DiagnosticResult<T>(T result) => new(result, default);
    public static implicit operator DiagnosticResult<T>(Optional<T> result) => new(result, default);
    public static implicit operator DiagnosticResult<T>(SequenceEquatableImmutableArray<DiagnosticData> diagnostics) => new(default, diagnostics);
    public static implicit operator DiagnosticResult<T>(ImmutableArray<DiagnosticData> diagnostics) => new(default, new(diagnostics));
    public static implicit operator DiagnosticResult<T>(DiagnosticData diagnostic) => new(default, new([diagnostic]));

    public DiagnosticResult<T> AppendDiagnostic(DiagnosticData diagnostic) => new(_result, _diags.Array.Add(diagnostic));

    public DiagnosticResult<T> AppendDiagnostics(IEnumerable<DiagnosticData> diagnostics) => new(_result, _diags.Array.AddRange(diagnostics));

    public DiagnosticResult<TResult> Select<TResult>(Func<T, TResult> selector)
        => new(_result.Select(selector), _diags);

    public DiagnosticResult<TResult> Bind<TResult>(Func<T, DiagnosticResult<TResult>> selector)
    {
        if (_result.HasValue) {
            var result = selector(_result.Value);
            return new(result._result, _diags.Array.AddRange(result._diags));
        }
        else {
            return new(default, _diags);
        }
    }

    public DiagnosticResult<TResult> Bind<TMid, TResult>(Func<T, DiagnosticResult<TMid>> selector, Func<T, TMid, TResult> resultSelector)
    {
        if (_result.HasValue) {
            var result = selector(_result.Value);
            if (result._result.HasValue) {
                return new(resultSelector(_result.Value, result._result.Value), _diags.Array.AddRange(result._diags));
            }
            else {
                return new(default, _diags.Array.AddRange(result._diags));
            }
        }
        else {
            return new(default, _diags);
        }
    }

    public DiagnosticResult<TResult> Zip<TOther, TResult>(DiagnosticResult<TOther> other, Func<T, TOther, TResult> resultSelector)
    {
        if (_result.HasValue && other._result.HasValue) {
            return new(resultSelector(_result.Value, other._result.Value), _diags.Array.AddRange(other._diags));
        }
        else {
            return new(default, _diags.Array.AddRange(other._diags));
        }
    }

    public DiagnosticResult<T> Where(Func<T, bool> predicate) => new(_result.Where(predicate), _diags);
}
