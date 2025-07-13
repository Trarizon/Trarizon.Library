#define OPTIONAL
#define EITHER

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;
public static class Result
{
    public static Result<T, TError> Success<T, TError>(T value)
        => new(value);

    public static ResultSuccessBuilder<T> Success<T>(T value)
        => new(value);

    public static Result<T, TError> Error<T, TError>(TError error)
        => new(error);

    public static ResultFailedBuilder<TError> Error<TError>(TError error)
        => new(error);

    public static T GetValueOrThrowError<T, TException>(this in Result<T, TException> result) where TException : Exception
    {
        if (!result.IsSuccess)
            ThrowException(result.Error);
        return result._value;

        [DoesNotReturn]
        static void ThrowException(Exception exception)
            => throw exception;
    }

    public static ref readonly T? GetValueRefOrDefaultRef<T, TError>(this ref readonly Result<T, TError> result)
        => ref result._value;

#if OPTIONAL

    public static Optional<T> ToOptional<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? Optional.Of(result._value) : default;

    public static Optional<TError> ToOptionalError<T, TError>(this in Result<T, TError> result)
        => result.IsError ? Optional.Of(result.Error) : default;

#endif

#if EITHER

    public static Either<T, TError> AsEitherLeft<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? new(result._value) : new(result.Error);

    public static Either<TError, T> AsEitherRight<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? new(result._value) : new(result.Error);

#endif

    [DoesNotReturn]
    internal static void ThrowResultIsError() => throw new InvalidOperationException("Result<> is error");

    internal class ValueTypeBox<T>(T value)
    {
        public readonly T Value = value;
    }
}

/// <summary>
/// Monad Result, Note that if TError is struct, the error value will be boxed
/// </summary>
public readonly struct Result<T, TError>
{
    internal readonly T? _value;
    internal readonly object? _error;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public bool IsSuccess => _error is null;

    [MemberNotNullWhen(false, nameof(_value))]
    [MemberNotNullWhen(true, nameof(_error), nameof(Error))]
    public bool IsError => _error is not null;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public TError? Error
    {
        get {
            if (typeof(TError).IsValueType) {
                Debug.Assert(_error is Result.ValueTypeBox<TError>);
                var box = Unsafe.As<Result.ValueTypeBox<TError>>(_error);
                if (box is null)
                    return default;
                return box.Value;
            }
            else {
                return Unsafe.As<object?, TError?>(ref Unsafe.AsRef(in _error));
            }
        }
    }

    public T GetValueOrThrow()
    {
        if (!IsSuccess)
            Result.ThrowResultIsError();
        return _value;
    }

    public T? GetValueOrDefault() => _value;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValueOrDefault(T? defaultValue)
        => IsSuccess ? _value : defaultValue;

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError error)
    {
        value = _value;
        error = Error;
        return IsSuccess;
    }

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsSuccess;
    }

    [MemberNotNullWhen(false, nameof(_value)), MemberNotNullWhen(true, nameof(_error), nameof(Error))]
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        error = Error;
        return IsError;
    }

    #endregion

    #region Creator

    private Result(T value, object? error)
    {
        _value = value;
        _error = error;
    }

    private Result(T? value, TError? error)
    {
        _value = value;

        if (typeof(TError).IsValueType) {
            _error = new Result.ValueTypeBox<TError>(error!);
        }
        else {
            _error = error;
        }
    }

    public Result(T value) : this(value, default(object)) { }

    public Result(TError error) : this(default, error) { }

    public static implicit operator Result<T, TError>(T value) => new(value);
    public static implicit operator Result<T, TError>(TError error) => new(error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(ResultSuccessBuilder<T> builder) => new(builder._value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, TError>(ResultFailedBuilder<TError> builder) => new(builder._error);

    #endregion

    #region Convertor

    public TResult Match<TResult>(Func<T, TResult> successSelector, Func<TError, TResult> errorSelector)
        => IsSuccess ? successSelector(_value) : errorSelector(Error);

    public void Match(Action<T>? successSelector, Action<TError>? errorSelector)
    {
        if (IsSuccess)
            successSelector?.Invoke(_value);
        else
            errorSelector?.Invoke(Error);
    }

    public void MatchValue(Action<T> selector)
    {
        if (IsSuccess) selector(_value);
    }

    public void MatchError(Action<TError> selector)
    {
        if (IsError) selector(Error);
    }

    public Result<TResult, TError> Select<TResult>(Func<T, TResult> selector)
        => IsSuccess ? new(selector(_value)) : new(Error);

    public Result<T, TResult> SelectError<TResult>(Func<TError, TResult> selector) 
        => IsSuccess ? new(_value) : new(selector(Error));

    public Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> selector)
        => IsSuccess ? selector(_value) : new(Error);

    #endregion

    public override string ToString()
    {
        if (IsSuccess) {
            var str = _value.ToString();
            return str is null ? "Result Value" : $"Value: {str}";
        }
        else {
            var str = _error.ToString();
            return str is null ? "Result Error" : $"Error: {str}";
        }
    }
}

#pragma warning disable CS0649

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct ResultSuccessBuilder<T>
{
    internal readonly T _value;

    internal ResultSuccessBuilder(T value) => _value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Build<TError>() => _value;
}

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct ResultFailedBuilder<TError>
{
    internal readonly TError _error;

    internal ResultFailedBuilder(TError error) => _error = error;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Build<T>() => _error;
}

#nullable restore
