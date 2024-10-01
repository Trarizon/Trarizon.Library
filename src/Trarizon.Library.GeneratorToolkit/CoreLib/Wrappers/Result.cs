﻿using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Wrappers;
public static class Result
{
    public static Result<T, TError> Success<T, TError>(T value) where TError : class
        => new(value);

    public static Result<T, TError> Failed<T, TError>(TError error) where TError : class
        => new(error);

    public static T GetValueOrThrow<T, TException>(this in Result<T, TException> result) where TException : Exception
    {
        if (!result.IsSuccess)
        {
            TraThrow.Exception(result._error);
            return default!;
        }
        return result._value;
    }

    public static ref readonly T? GetValueRefOrDefaultRef<T, TError>(this ref readonly Result<T, TError> result) where TError : class
        => ref result._value;

    #region Conversion

    public static Optional<T> ToOptional<T, TError>(this in Result<T, TError> result) where TError : class
        => result.IsSuccess ? Optional.Of(result._value) : default;

    public static Either<T, TError> AsEitherLeft<T, TError>(this in Result<T, TError> result) where TError : class
        => result.IsSuccess ? new(result._value) : new(result._error);

    public static Either<TError, T> AsEitherRight<T, TError>(this in Result<T, TError> result) where TError : class
        => result.IsSuccess ? new(result._value) : new(result._error);

    #endregion
}

public readonly struct Result<T, TError>
#if NET9_0_OR_GREATER
    where T : allows ref struct
#endif
    where TError : class
{
    internal readonly T? _value;
    internal readonly TError? _error;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public bool IsSuccess => _error is null;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public TError? Error => _error;

    public T GetValidValue()
    {
        if (!IsSuccess)
            ThrowHelper.ThrowInvalidOperationException($"Result<> failed.");
        return _value;
    }

    public T? GetValueOrDefault() => _value;

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError error)
    {
        value = _value;
        error = _error;
        return IsSuccess;
    }

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsSuccess;
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
        => IsSuccess ? new(selector(_value)) : new(_error);

    public Result<TResult, TError> SelectWrapped<TResult>(Func<T, Result<TResult, TError>> selector)
        => IsSuccess ? selector(_value) : new(_error);

    #endregion

    public override string ToString()
    {
        if (IsSuccess)
        {
            var str = _value.ToString();
            return str is null ? "Result Value" : $"Value: {str}";
        }
        else
        {
            var str = _error.ToString();
            return str is null ? "Result Error" : $"Error: {str}";
        }
    }
}