using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

#if NET9_0_OR_GREATER

public static partial class RefResult
{
    extension(Result)
    {
        public static RefResultVoid<Exception> TryCatch(Action action)
        {
            try { action(); return Result.Success(); }
            catch (Exception ex) { return ex; }
        }
    }

    public static ref readonly TError? GetErrorRefOrDefaultRef<TError>(this ref readonly RefResultVoid<TError> result)
        where TError : allows ref struct
        => ref result._error;

    public static ResultVoid<TError> AsDeref<TError>(this RefResultVoid<TError> self) => self;
    public static RefResultVoid<TError> AsRef<TError>(this ResultVoid<TError> self) => self;
}

public readonly ref struct RefResultVoid<TError>
    where TError : allows ref struct
{
    internal readonly bool _failure;
    internal readonly TError? _error;

    public static RefResultVoid<TError> Success => default;

    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess => !_failure;

    [MemberNotNullWhen(true, nameof(_error))]
    public bool IsFailure => _failure;

    public TError Error => _error!;

    public TError GetErrorOrThrow()
    {
        if (IsSuccess)
            ResultException.ThrowVoid(_error);
        return _error;
    }

    public TError? GetErrorOrDefault() => _error;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public TError? GetErrorOrDefault(TError? defaultValue)
        => IsFailure ? _error : defaultValue;

    [MemberNotNullWhen(true, nameof(_error))]
    public bool TryGetError([MaybeNullWhen(false)] out TError? error)
    {
        error = _error;
        return IsFailure;
    }

    internal RefResultVoid(bool failure, TError? error)
    {
        _failure = failure;
        _error = error;
    }

    public RefResultVoid(TError error)
    {
        _failure = true;
        _error = error;
    }

    public static implicit operator RefResultVoid<TError>(TError error) => new(error);
    public static implicit operator RefResultVoid<TError>(Result.SuccessBuilder<Unit> builder) => default;
    public static implicit operator RefResultVoid<TError>(RefResult.SuccessBuilder<Unit> builder) => default;
    public static implicit operator RefResultVoid<TError>(RefResult.FailureBuilder<TError> buiilder) => new(buiilder._error);

    public static implicit operator RefResultVoid<TError>(RefResult<Unit, TError> result) => new(result.IsFailure, result.Error);
    public static implicit operator RefResult<Unit, TError>(RefResultVoid<TError> result) => new(result.IsSuccess, Unit.Value, result.Error);

#pragma warning disable CS0809

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Not supported for ref struct")]
    public override string ToString() => throw new NotSupportedException();

#pragma warning restore CS0809
}

partial struct ResultVoid<TError>
{
    public static implicit operator RefResultVoid<TError>(ResultVoid<TError> result) => new(result._failure, result._error);
    public static implicit operator ResultVoid<TError>(RefResultVoid<TError> result) => new(result._failure, result._error);
}

#endif
