using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Wrappers;
public static class Result
{
    public static Result<T, TError> Success<T, TError>(T value) where TError : class
        => new(value);

    public static Result<T, TError> Failed<T, TError>(TError error) where TError : class
        => new(error);

    public static void ThrowIfFailed<T, TException>(this Result<T, TException> result) where TException : Exception
    {
        if (!result.Success)
            throw result.Error;
    }

    #region Conversion

    public static Optional<T> ToOptional<T, TError>(this Result<T, TError> result) where TError : class
        => result.Success ? Optional.Of(result._value) : default;

    public static Either<T, TError> AsEitherLeft<T, TError>(this Result<T, TError> result) where TError : class
        => result.Success ? new(result._value) : new(result._error);

    public static Either<TError, T> AsEitherRight<T, TError>(this Result<T, TError> result) where TError : class
        => result.Failed ? new(result._error) : new(result._value);

    #endregion
}

public readonly struct Result<T, TError> where TError : class
{
    internal readonly T? _value;
    internal readonly TError? _error;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value), nameof(Value))]
    [MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public readonly bool Success => _error is null;

    [MemberNotNullWhen(false, nameof(_value), nameof(Value))]
    [MemberNotNullWhen(true, nameof(_error), nameof(Error))]
    public readonly bool Failed => _error is not null;

    public readonly T Value
    {
        get
        {
            return _value!;
        }
    }

    [NotNull]
    public readonly TError Error
    {
        get
        {
            return _error!;
        }
    }

    public readonly T? GetValueOrDefault() => _value;
    public readonly TError? GetErrorOrDefault() => _error;

    [MemberNotNullWhen(true, nameof(_value), nameof(Value))]
    [MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError error)
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

    private Result(T? value, TError? error) { _value = value; _error = error; }

    public Result(T value) : this(value, null) { }

    public Result(TError error) : this(default, error) { }

    public static implicit operator Result<T, TError>(T value) => new(value);
    public static implicit operator Result<T, TError>(TError error) => new(error);

    #endregion

    #region Convertor

    public Result<TResult, TError> Select<TResult>(Func<T, TResult> selector)
        => Success ? new(selector(_value)) : new(_error);

    public Result<TResult, TNewError> Select<TResult, TNewError>(Func<T, TResult> valueSelector, Func<TError, TNewError> errorSelector) where TNewError : class
        => Success ? new(valueSelector(_value)) : new(errorSelector(_error));

    public Result<TResult, TError> SelectWrapped<TResult>(Func<T, Result<TResult, TError>> selector)
        => Success ? selector(_value) : new(_error);

    public Result<T, TNewError> SelectError<TNewError>(Func<TError, TNewError> selector) where TNewError : class
        => Success ? new(_value) : new(selector(_error));
    #endregion

    public override string ToString() => Success ? $"Success: {_value}" : $"Failed: {_error}";
}
