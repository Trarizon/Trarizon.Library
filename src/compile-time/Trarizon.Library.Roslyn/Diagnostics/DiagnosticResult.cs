using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Trarizon.Library.Functional;

namespace Trarizon.Library.Roslyn.Diagnostics;
public readonly struct DiagnosticResult<T>
{
    private readonly IEnumerable<DiagnosticData>? _diags;
    private readonly Optional<T> _value;

    public Optional<T> ValueOptional => _value;

    public bool HasValue => _value.HasValue;

    public T Value => _value.GetValueOrThrow();

    public DiagnosticResultDiagnostics Diagnostics => new(_diags);

    private DiagnosticResult(Optional<T> value, IEnumerable<DiagnosticData>? diagnostics)
    {
        _value = value;
        _diags = diagnostics;
    }

    public DiagnosticResult(T value, ReadOnlySpan<DiagnosticData> diagnostics)
        : this(Optional.Of(value), diagnostics.ToArray())
    { }

    public DiagnosticResult(T value, ImmutableArray<DiagnosticData> diagnostics)
        : this(Optional.Of(value), ImmutableCollectionsMarshal.AsArray(diagnostics))
    { }

    public DiagnosticResult(T value) : this(Optional.Of(value), []) { }

    public DiagnosticResult(DiagnosticData diagnostic) : this(Optional.None, [diagnostic]) { }
    public DiagnosticResult(ReadOnlySpan<DiagnosticData> diagnostics) : this(Optional.None, diagnostics.ToArray()) { }
    public DiagnosticResult(ImmutableArray<DiagnosticData> diagnostics) : this(Optional.None, ImmutableCollectionsMarshal.AsArray(diagnostics)) { }
    public DiagnosticResult(IEnumerable<DiagnosticData>? diagnostics) : this(Optional.None, diagnostics) { }

    public static implicit operator DiagnosticResult<T>(T result) => new(result);
    public static implicit operator DiagnosticResult<T>(DiagnosticData diagnostic) => new(diagnostic);
    public static implicit operator DiagnosticResult<T>(ReadOnlySpan<DiagnosticData> diagnostics) => new(diagnostics);
    public static implicit operator DiagnosticResult<T>(ImmutableArray<DiagnosticData> diagnostics) => new(diagnostics);

    public static implicit operator DiagnosticResult<T>(DiagnosticResultDiagnostics diags) => new(diags._diags);

    public DiagnosticResult<T> WithValue(T value) => new(value, _diags);

    public DiagnosticResult<T> AppendDiagnostics(ReadOnlySpan<DiagnosticData> diagnostics)
    {
        var diags = CombineDiagnostics(_diags, diagnostics);
        return new(_value, diags);
    }

    public DiagnosticResult<T> AppendDiagnostics(IEnumerable<DiagnosticData> diagnostics)
    {
        var diags = CombineDiagnostics(_diags, diagnostics);
        return new(_value, diags);
    }

    public DiagnosticResult<T> AppendDiagnostic(DiagnosticData diagnostic)
    {
        var diags = CombineDiagnostics(_diags, diagnostic);
        return new(_value, diags);
    }

    private static IEnumerable<DiagnosticData>? CombineDiagnostics(IEnumerable<DiagnosticData>? src, ReadOnlySpan<DiagnosticData> other)
    {
        if (other.IsEmpty)
            return src;

        if (src is null)
            return other.ToArray();

        if (src is DiagnosticData diag) {
            var arr = new DiagnosticData[1 + other.Length];
            arr[0] = diag;
            other.CopyTo(arr.AsSpan(1));
            return arr;
        }

        return src.Concat(other.ToArray());
    }

    private static IEnumerable<DiagnosticData>? CombineDiagnostics(IEnumerable<DiagnosticData>? src, IEnumerable<DiagnosticData>? other)
    {
        if (other is null)
            return src;

        if (src is null)
            return other;

        if (src is DiagnosticData diag) {
            if (other is DiagnosticData otherDiag) {
                return new[] { diag, otherDiag };
            }
            else {
                return other.Prepend(diag);
            }
        }
        else {
            if (other is DiagnosticData otherDiag) {
                return src.Append(otherDiag);
            }
            else {
                return src.Concat(other);
            }
        }
    }

    /// <summary>
    /// Combine diagnostics, and return <paramref name="other"/>'s value as new value if both this and <paramref name="other"/> have values
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public DiagnosticResult<T> AndThen(DiagnosticResult<T> other)
    {
        var diags = CombineDiagnostics(_diags, other._diags);
        if (HasValue && other.HasValue) {
            return new(other._value, diags);
        }
        else {
            return new(Optional.None, diags);
        }
    }

    public DiagnosticResult<T> OrThen(DiagnosticResult<T> other)
    {
        var diags = CombineDiagnostics(_diags, other._diags);
        if (other.HasValue)
            return new(other._value, diags);
        if (HasValue)
            return new(_value, diags);
        return new(Optional.None, diags);
    }

    public DiagnosticResult<TResult> Match<TResult>(Func<T, TResult> selector, Func<TResult> noValueSelector)
        => new(_value.Match(selector, noValueSelector), _diags);

    public DiagnosticResult<TResult> Select<TResult>(Func<T, TResult> selector)
        => new(_value.Select(selector), _diags);

    public DiagnosticResult<TResult> SelectValue<TResult>(TResult value)
        => new(value, _diags);

    public DiagnosticResult<TResult> Bind<TResult>(Func<T, DiagnosticResult<TResult>> selector)
    {
        if (_value.TryGetValue(out var val)) {
            var result = selector(val);
            var diags = CombineDiagnostics(_diags, result._diags);
            if (result.HasValue) {
                return new(result._value, diags);
            }
            else {
                return new(Optional.None, diags);
            }
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
                var diags = CombineDiagnostics(_diags, mid.Diagnostics);
                return new(res, diags);
            }
            else {
                var diags = CombineDiagnostics(_diags, mid.Diagnostics);
                return new(Optional.None, diags);
            }
        }
        return new(Optional.None, _diags);
    }

    public DiagnosticResult<TResult> Zip<T2, TResult>(DiagnosticResult<T2> other, Func<T, T2, TResult> resultSelector)
    {
        var diags = CombineDiagnostics(_diags, other.Diagnostics);
        return new(_value.Zip(other._value, resultSelector), diags);
    }

    public DiagnosticResult<T> Where(Func<T, bool> predicate)
        => new(_value.Where(predicate), _diags);
}

public readonly struct DiagnosticResultDiagnostics : IEnumerable<DiagnosticData>
{
    internal readonly IEnumerable<DiagnosticData>? _diags;

    internal DiagnosticResultDiagnostics(IEnumerable<DiagnosticData>? diags)
        => _diags = diags;

    public DiagnosticResult<T> Build<T>() => new(_diags);

    public IEnumerator<DiagnosticData> GetEnumerator() => (_diags ?? []).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => (_diags ?? []).GetEnumerator();
}