using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Trarizon.Library.Functional.Monads;

namespace Trarizon.Library.Roslyn.Diagnostics;
public readonly struct DiagnosticResult<T>
{
    private readonly Optional<T> _value;
    private readonly DiagnosticData[]? _diags;

    public Optional<T> Value => _value;

    public IReadOnlyList<DiagnosticData> Diagnostics => _diags ?? [];

    private DiagnosticResult(Optional<T> value, DiagnosticData[]? diagnostics)
    {
        _value = value;
        _diags = diagnostics ?? [];
    }

    public DiagnosticResult(T value, params DiagnosticData[]? diagnostics) : this(Optional.Of(value), diagnostics) { }

    public DiagnosticResult(T value) : this(value, []) { }
    public DiagnosticResult(DiagnosticData diagnostic) : this(new DiagnosticData[] { diagnostic }) { }
    public DiagnosticResult(DiagnosticData[] diagnostics) : this(default(Optional<T>), diagnostics) { }
    public DiagnosticResult(ImmutableArray<DiagnosticData> diagnostics) : this(default(Optional<T>), ImmutableCollectionsMarshal.AsArray(diagnostics) ?? []) { }

    public static implicit operator DiagnosticResult<T>(T result) => new(result);
    public static implicit operator DiagnosticResult<T>(DiagnosticData diagnostic) => new(diagnostic);
    public static implicit operator DiagnosticResult<T>(DiagnosticData[] diagnostics) => new(diagnostics);
    public static implicit operator DiagnosticResult<T>(ImmutableArray<DiagnosticData> diagnostics) => new(diagnostics);

    public DiagnosticResult<T> AddDiagnostics(params ReadOnlySpan<DiagnosticData> diagnostics)
    {
        if (diagnostics.Length == 0)
            return this;
        return new(_value, [.. _diags ?? [], .. diagnostics]);
    }

    /// <summary>
    /// Combine diagnostics, and return <paramref name="other"/>'s value as new value
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public DiagnosticResult<TResult> CombineTo<TResult>(DiagnosticResult<TResult> other)
    {
        if (other._diags?.Length > 0)
            return new(other._value, [.. _diags ?? [], .. other._diags ?? []]);
        return new(other._value, _diags);
    }

    public DiagnosticResult<TResult> Match<TResult>(Func<T, TResult> selector, Func<TResult> noValueSelector)
        => new(_value.Match(selector, noValueSelector), _diags);

    public DiagnosticResult<TResult> Select<TResult>(Func<T, TResult> selector)
        => new(_value.Select(selector), _diags);

    public DiagnosticResult<TResult> Bind<TResult>(Func<T, DiagnosticResult<TResult>> selector)
    {
        if (_value.TryGetValue(out var val)) {
            var result = selector(val);
            return CombineTo(result);
        }
        else {
            return new(Optional.None, _diags);
        }
    }

    public DiagnosticResult<TResult> Bind<TMid, TResult>(Func<T, DiagnosticResult<TMid>> selector, Func<T, TMid, TResult> resultSelector)
    {
        if (_value.TryGetValue(out var val)) {
            var mid = selector(val);
            if (mid._value.TryGetValue(out var m)) {
                var res = resultSelector(val, m);
                return new(res, [.. _diags ?? [], .. mid._diags ?? []]);
            }
            else {
                if (mid._diags?.Length > 0)
                    return new(Optional.None, [.. _diags ?? [], .. mid._diags]);
            }
        }

        return new(Optional.None, _diags);
    }

    public DiagnosticResult<TResult> Zip<T2, TResult>(DiagnosticResult<T2> other, Func<T, T2, TResult> resultSelector)
    {
        DiagnosticData[]? diags;
        if (_diags?.Length > 0)
            diags = other._diags;
        else if (other._diags?.Length > 0)
            diags = _diags;
        else
            diags = [.. _diags ?? [], .. other._diags ?? []];

        return new(_value.Zip(other._value, resultSelector), diags);
    }
}
