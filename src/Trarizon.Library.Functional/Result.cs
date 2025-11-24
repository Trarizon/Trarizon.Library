#if RESULT

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

    public static SuccessBuilder<T> Success<T>(T value)
        => new(value);

    public static Result<T, TError> Failure<T, TError>(TError error)
        => new(error);

    public static FailureBuilder<TError> Failure<TError>(TError error)
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

    public static ref readonly TError? GetErrorRefOrDefaultRef<T, TError>(this ref readonly Result<T, TError> result)
        => ref BoxHelpers.UnboxRef<TError>(in result._error);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct SuccessBuilder<T>
    {
        internal readonly T _value;
        internal SuccessBuilder(T value) => _value = value;
        public Result<T, TError> Build<TError>() => _value;

        public bool IsSuccess => true;
        public bool IsFailure => false;
        public T Value => _value;
        public FailureBuilder<T> Swap() => new(_value);
        public SuccessBuilder<TResult> Cast<TResult>() => new((TResult)(object)_value!);
        public SuccessBuilder<TResult> Select<TResult>(Func<T, TResult> selector) => new(selector(_value));
        public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
        public override string ToString() => Build<object>().ToString();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct FailureBuilder<TError>
    {
        internal readonly TError _error;
        internal FailureBuilder(TError error) => _error = error;
        public Result<T, TError> Build<T>() => _error;

        public bool IsSuccess => false;
        public bool IsFailure => true;
        public TError Error => _error;
        public SuccessBuilder<TError> Swap() => new(_error);
        public FailureBuilder<TNewError> CastError<TNewError>() => new((TNewError)(object)_error!);
        public FailureBuilder<TNewError> SelectError<TNewError>(Func<TError, TNewError> selector) => new(selector(_error));
        public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
        public override string ToString() => Build<object>().ToString();
    }
}

/// <summary>
/// Monad Result, Note that if TError is struct, the error value will be boxed, some boxes will be cached
/// </summary>
public readonly partial struct Result<T, TError>
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

    public TError? Error => BoxHelpers.Unbox<TError>(_error);

    public T GetValueOrThrow()
    {
        if (!IsSuccess)
            ResultErrorException.Throw<T, TError>(Error);
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
        Debug.Assert(BoxHelpers.IsValidBox<TError>(error));
        _error = error;
    }

    private Result(T? value, TError? error)
    {
        _value = value;
        _error = BoxHelpers.Box(error);
    }

    public Result(T value) : this(value, default(object)) { }

    public Result(TError error) : this(default, error) { }

    public static implicit operator Result<T, TError>(T value) => new(value);
    public static implicit operator Result<T, TError>(TError error) => new(error);
    public static implicit operator Result<T, TError>(Result.SuccessBuilder<T> builder) => new(builder._value);
    public static implicit operator Result<T, TError>(Result.FailureBuilder<TError> builder) => new(builder._error);

    #endregion

    #region Convertor

    public Result<TError, T> Swap() => new(Error, _value);

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

    public Result<T, TNewError> BindError<TNewError>(Func<TError, Result<T, TNewError>> selector)
        => IsSuccess ? new(_value) : selector(Error);

    #endregion

    public override string ToString() => (IsSuccess ? _value.ToString() : Error.ToString()) ?? "";

    public string ToString(bool includeVariantInfo)
    {
        if (!includeVariantInfo) {
            return ToString();
        }

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
            if (typeof(TError).IsValueType) {
                var box = Unsafe.As<ValueBox<TError>>(_error);
                str = box.Value is IMonad monad ? monad.ToString(true) : box.ToString() ?? "";
            }
            else if (Error is IMonad monad)
                str = monad.ToString(true);
            else
                str = Error.ToString();
            return str is null ? "Result Error" : $"Error({str})";
        }
    }
}

public sealed class ResultErrorException : InvalidOperationException
{
    public object Error { get; }

    private ResultErrorException(Type valueType, Type errorType, object error)
        : base($"Result<{valueType.Name}, {errorType.Name}> is Error({error})")
        => Error = error;

    [DoesNotReturn]
    public static void Throw<T, TError>(TError error) => throw new ResultErrorException(typeof(T), typeof(TError), error!);
}

#endif
