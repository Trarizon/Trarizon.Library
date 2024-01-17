using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Wrappers;
public static class Result
{
    public static Result<T, TError> Success<T, TError>(T value) where TError : class
        => new(value, null);

    public static Result<T, TError> Failed<T, TError>(TError error) where TError : class
        => new(default, error);

    public static void ThrowIfFailed<T, TException>(this Result<T, TException> result) where TException : Exception
    {
        if (!result.Success)
            throw result.Error;
    }
}

public readonly struct Result<T, TError> where TError : class
{
    private readonly T? _value;
    private readonly TError? _error;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value), nameof(Value))]
    [MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public readonly bool Success => _error is null;

    public readonly bool Failed => _error is not null;

    public readonly T Value
    {
        get {
            if (!Success)
                ThrowHelper.ThrowInvalidOperation($"Result<> failed.");
            return _value;
        }
    }

    [NotNull]
    public readonly TError Error
    {
        get {
            if (Success)
                ThrowHelper.ThrowInvalidOperation($"Cannot get error, Result<> is success.");
            return _error;
        }
    }

    public readonly T? GetValueOrDefault() => _value;
    public readonly TError? GetErrorOrDefault() => _error;

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError? error)
    {
        value = _value;
        error = _error;
        return Success;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return Success;
    }

    #endregion

    #region Creator

    internal Result(T? value, TError? error) { _value = value; _error = error; }

    public Result(T value) : this(value, null) { }

    public Result(TError? error) : this(default, error) { }

    public static implicit operator Result<T, TError>(T value) => new(value, null);
    public static implicit operator Result<T, TError>(TError error) => new(default, error);

    #endregion

    #region Linq

    public Result<TResult, TError> Select<TResult>(Func<T, TResult> selector)
        => Success ? new(selector(_value), null) : new(default, _error);

    public Result<TResult, TError> SelectWrapped<TResult>(Func<T, Result<TResult, TError>> selector)
        => Success ? selector(_value) : new(default, _error);

    public Result<T, TNewError> SelectError<TNewError>(Func<TError, TNewError> selector) where TNewError : class
        => Success ? new(_value, default) : new(default, selector(_error));

    #endregion

    public override string ToString() => Success ? $"Success: {_value}" : $"Failed: {_error}";
}
