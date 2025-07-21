//#define OPTIONAL
//#define EITHER

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

    public static Optional<Result<T, TError>> Transpose<T, TError>(this in Result<Optional<T>, TError> result)
    {
        if (result.IsSuccess) {
            ref readonly var optional = ref result._value;
            return optional.HasValue ? Optional.Of(Result.Success<T, TError>(optional._value)) : Optional.None;
        }
        else {
            return Optional.Of(Result.Error<T, TError>(result.Error));
        }
    }

#endif

#if EITHER

    public static Either<T, TError> AsEitherLeft<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? new(result._value) : new(result.Error);

    public static Either<TError, T> AsEitherRight<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? new(result._value) : new(result.Error);

#endif

    [DoesNotReturn]
    internal static void ThrowResultIsError<TError>(TError error) => throw new ResultErrorException<TError>(error);

    internal class ValueTypeBox<T>(T value)
    {
        public readonly T Value = value;

        public string ToString(bool includeVariantInfo)
        {
            if (Value is IMonad monad)
                return monad.ToString(includeVariantInfo);
            return Value!.ToString() ?? "";
        }

        public override string? ToString() => Value!.ToString();
    }
}

/// <summary>
/// Monad Result, Note that if TError is struct, the error value will be boxed
/// </summary>
public readonly struct Result<T, TError> : IMonad
{
    internal readonly T? _value;
    // For reference type, we store the TError
    // For value type, we box it into Result.ValueTypeBox<T>
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
            Result.ThrowResultIsError(Error);
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

    private Result(T? value, object? error)
    {
        _value = value;
        Debug.Assert(error is null || error is TError || (typeof(T).IsValueType && error is Result.ValueTypeBox<TError>));
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
        => IsSuccess ? new(selector(_value)) : new(default!, _error);

    public Result<T, TResult> SelectError<TResult>(Func<TError, TResult> selector)
        => IsSuccess ? new(_value) : new(selector(Error));

    public Result<TResult, TResultError> Select<TResult, TResultError>(Func<T, TResult> valueSelector, Func<TError, TResultError> errorSelector)
        => IsSuccess ? new(valueSelector(_value)) : new(errorSelector(Error));

    public Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> selector)
        => IsSuccess ? selector(_value) : new(default!, _error);

    #endregion

    public override string ToString() => ToString(includeVariantInfo: false);

    public string ToString(bool includeVariantInfo)
    {
        if (includeVariantInfo) {
            if (IsSuccess) {
                string? str;
                if (_value is IMonad monad)
                    str = monad.ToString(true);
                else
                    str = _value.ToString();
                return str is null ? "Result Success" : $"Success({str})";
            }
            else {
                string? str;
                if (typeof(TError).IsValueType)
                    str = Unsafe.As<Result.ValueTypeBox<TError>>(_error).ToString(true);
                else if (Error is IMonad monad)
                    str = monad.ToString(true);
                else
                    str = Error.ToString();
                return str is null ? "Result Error" : $"Error({str})";
            }
        }
        else {
            if (IsSuccess) {
                return _value.ToString() ?? "";
            }
            else {
                return Error.ToString() ?? "";
            }
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

public sealed class ResultErrorException<TError>(TError error)
    : InvalidOperationException($"Result<,> is Error{(error is null ? "" : $"({error})")}")
{
    public TError Error => error;
}
