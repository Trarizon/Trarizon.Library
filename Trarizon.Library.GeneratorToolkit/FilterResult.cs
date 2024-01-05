using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit;
public readonly struct FilterResult
{
    public static FilterResult Create(Diagnostic diagnostic, bool closeWhenError = false) => new(closeWhenError, diagnostic);

    public static FilterResult<T> Create<T>(T value) => new(value, null);
    public static FilterResult<T> Create<T>(Diagnostic diagnostic) => new(default, diagnostic);

    private FilterResult(bool close, Diagnostic diagnostic)
        => (WillClose, Diagnostic) = (close, diagnostic);

    internal readonly bool WillClose;
    internal readonly Diagnostic Diagnostic;
}

public readonly struct FilterResult<T>
{
    internal readonly T Value;
    internal readonly Diagnostic? Diagnostic;

    [MemberNotNullWhen(true, nameof(Diagnostic))]
    [MemberNotNullWhen(false, nameof(Value))]
    internal bool Error => Diagnostic is not null;

    internal FilterResult(T? value, Diagnostic? diagnostic)
        => (Value, Diagnostic) = (value!, diagnostic);

    public static implicit operator FilterResult<T>(T value) => new(value, null);
    public static implicit operator FilterResult<T>(Diagnostic diagnostic) => new(default!, diagnostic);
}