using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Functional.Abstraction;

namespace Trarizon.Library.Functional;

public static partial class Result
{
    public static SuccessBuilder<Unit> Success()
        => new(Unit.Value);

    public static ResultVoid<Exception> TryCatch(Action func)
    {
        try { func(); return Success(); }
        catch (Exception ex) { return ex; }
    }

    public static ref readonly TError? GetErrorRefOfDefaultRef<TError>(this ref readonly ResultVoid<TError> result)
        => ref result._error;
}

public readonly partial struct ResultVoid<TError>
    : ITaggedUnion
{
    private readonly bool _failure;
    internal readonly TError? _error;

    public static ResultVoid<TError> Success => default;

    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess => !_failure;

    [MemberNotNullWhen(true, nameof(_error))]
    public bool IsFailure => _failure;

    public TError Error => _error!;

    public TError GetErrorOrThrow()
    {
        if (IsSuccess)
            ResultException.ThrowVoid(_error!);
        return _error;
    }

    public TError? GetErrorOrDefault() => _error;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public TError? GetErrorOrDefault(TError? defaultValue)
        => IsFailure ? _error : defaultValue;

    [MemberNotNullWhen(true, nameof(_error))]
    public bool TryGetError(out TError? error)
    {
        error = _error;
        return IsFailure;
    }

    private ResultVoid(bool failure, TError? error)
    {
        _failure = failure;
        _error = error;
    }

    public ResultVoid(TError error)
    {
        _failure = true;
        _error = error;
    }

    public static implicit operator ResultVoid<TError>(TError error) => new(error);
    public static implicit operator ResultVoid<TError>(Result.SuccessBuilder<Unit> builder) => default;
    public static implicit operator ResultVoid<TError>(Result.FailureBuilder<TError> builder) => new(builder._error);

    public static implicit operator ResultVoid<TError>(Result<Unit, TError> result) => new(result.IsFailure, result.Error);
    public static implicit operator Result<Unit, TError>(ResultVoid<TError> result) => new(result.IsSuccess, Unit.Value, result.Error);


    public ResultVoid<TNewError> CastError<TNewError>() => IsFailure ? new((TNewError)(object)_error) : default;

    public override string ToString() => IsFailure ? _error.ToString() ?? "" : "";

    public string ToString(bool includeVariantInfo)
    {
        if (!includeVariantInfo) {
            return ToString();
        }
        if (IsSuccess) {
            return "Result Success";
        }
        else {
            string? str;
            if (_error is ITaggedUnion monad)
                str = monad.ToString(true);
            else
                str = _error.ToString();
            return str is null ? "Result Error" : $"Error({str})";
        }
    }

    public static bool operator ==(ResultVoid<TError> left, ResultVoid<TError> right)
    {
        return (left.IsSuccess, right.IsSuccess) switch
        {
            (true, true) => true,
            (false, false) => EqualityComparer<TError>.Default.Equals(left.Error, right.Error),
            _ => false,
        };
    }
    public static bool operator !=(ResultVoid<TError> left, ResultVoid<TError> right) => !(left == right);
    public bool Equals(ResultVoid<TError> other) => this == other;
    public override bool Equals(object? obj) => obj is ResultVoid<TError> other && this == other;
    public override int GetHashCode() => IsSuccess ? 0 : _error.GetHashCode();
}

partial class ResultException
{
    [DoesNotReturn]
    public static void ThrowVoid<TError>(TError error)
#if NET9_0_OR_GREATER
        where TError : allows ref struct
#endif
        => throw new ResultException(typeof(void), typeof(TError), true);
}