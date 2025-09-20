//#define MONAD
//#define OPTIONAL
//#define EITHER
//#define EXT_ENUMERABLE

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Functional.Internal;

namespace Trarizon.Library.Functional;
public static partial class Result
{
    public static Result<T, TError> Success<T, TError>(T value)
        => new(value);

    public static ResultSuccessBuilder<T> Success<T>(T value)
        => new(value);

    public static Result<T, TError> Failure<T, TError>(TError error)
        => new(error);

    public static ResultFailedBuilder<TError> Failure<TError>(TError error)
        => new(error);

    public static Result<T, TError> Create<T, TError>(bool isSuccess, T value, TError error) 
        => isSuccess ? new(value) : new(error);

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
        => result.IsFailure ? Optional.Of(result.Error) : default;

    public static Optional<Result<T, TError>> Transpose<T, TError>(this in Result<Optional<T>, TError> result)
    {
        if (result.IsSuccess) {
            ref readonly var optional = ref result._value;
            return optional.HasValue ? Optional.Of(Result.Success<T, TError>(optional._value)) : Optional.None;
        }
        else {
            return Optional.Of(Result.Failure<T, TError>(result.Error));
        }
    }

#endif

#if EITHER

    public static Either<T, TError> AsEitherLeft<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? new(result._value) : new(result.Error);

    public static Either<TError, T> AsEitherRight<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? new(result._value) : new(result.Error);

#endif

#if EXT_ENUMERABLE

    public static IEnumerable<T> WhereIsSuccess<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsSuccess).Select(x => x.Value);

    public static IEnumerable<TError> WhereIsFailure<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsFailure).Select(x => x.Error!);

#endif

    [DoesNotReturn]
    internal static void ThrowResultIsError<TError>(TError error) => throw new ResultErrorException<TError>(error);

    internal static class ErrorHelper
    {
        public static string ToStringIncludeVariantInfo<T>(ValueTypeBox<T> value)
        {
            if (value.Value is IMonad monad)
                return monad.ToString(true);
            return value.ToString() ?? "";
        }
    }
}

/// <summary>
/// Monad Result, Note that if TError is struct, the error value will be boxed, some boxes will be cached
/// </summary>
public readonly struct Result<T, TError>
#if MONAD
    : IMonad
#endif
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
    public bool IsFailure => _error is not null;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public TError? Error => ValueTypeBox.GetValueOrDefault<TError>(_error);

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
        return IsFailure;
    }

    #endregion

    #region Creator

    private Result(T? value, object? error)
    {
        _value = value;
        Debug.Assert(ValueTypeBox.IsValueTypeBoxOrReferenceObject<TError>(error));
        _error = error;
    }

    private Result(T? value, TError? error)
    {
        _value = value;
        _error = ValueTypeBox.BoxIfValueType(error);
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

    public Result<TResult, TError> Cast<TResult>() => IsSuccess ? new((TResult)(object)_value) : new(default!, _error);

    public Result<T, TNewError> CastError<TNewError>() => IsFailure ? new(default!, (TNewError)(object)Error) : new(_value);

    public Result<TResult, TNewError> Cast<TResult, TNewError>() => IsSuccess ? new((TResult)(object)_value) : new(default!, (TNewError)(object)Error);

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
        if (IsFailure) selector(Error);
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
                    str = Result.ErrorHelper.ToStringIncludeVariantInfo(Unsafe.As<ValueTypeBox<TError>>(_error));
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

    public bool IsSuccess => true;
    public bool IsFailure => false;
    public T Value => _value;
    public ResultSuccessBuilder<TResult> Cast<TResult>() => new((TResult)(object)_value!);
    public void MatchValue(Action<T> selector) => selector(_value);
    public ResultSuccessBuilder<TResult> Select<TResult>(Func<T, TResult> selector) => new(selector(_value));
    public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
    public override string ToString() => ToString(includeVariantInfo: false);
}

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct ResultFailedBuilder<TError>
{
    internal readonly TError _error;

    internal ResultFailedBuilder(TError error) => _error = error;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Build<T>() => _error;

    public bool IsSuccess => false;
    public bool IsFailure => true;
    public TError Error => _error;
    public ResultFailedBuilder<TNewError> CastError<TNewError>() => new((TNewError)(object)_error!);
    public void MatchError(Action<TError> selector) => selector(_error);
    public ResultFailedBuilder<TNewError> SelectError<TNewError>(Func<TError, TNewError> selector) => new(selector(_error));
    public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
    public override string ToString() => ToString(includeVariantInfo: false);
}

#nullable restore

public sealed class ResultErrorException<TError>(TError error)
    : InvalidOperationException($"Result<,> is Error{(error is null ? "" : $"({error})")}")
{
    public TError Error => error;
}
