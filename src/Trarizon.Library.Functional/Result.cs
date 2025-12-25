using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Functional.Abstraction;

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

    public static Result<T, Exception> TryCatch<T>(Func<T> func)
    {
        try { return func(); }
        catch (Exception ex) { return ex; }
    }

    public static T GetValueOrThrowError<T, TException>(this in Result<T, TException> result) where TException : Exception
    {
        if (!result.IsSuccess)
            ThrowException(result._error);
        return result._value;

        [DoesNotReturn]
        static void ThrowException(Exception exception)
            => throw exception;
    }

    public static ref readonly T? GetValueRefOrDefaultRef<T, TError>(this ref readonly Result<T, TError> result)
        => ref result._value;

    public static ref readonly TError? GetErrorRefOrDefaultRef<T, TError>(this ref readonly Result<T, TError> result)
        => ref result._error;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly partial struct SuccessBuilder<T>
    {
        internal readonly T _value;
        internal SuccessBuilder(T value) => _value = value;
        public Result<T, TError> Build<TError>() => _value;

        public bool IsSuccess => true;
        public bool IsFailure => false;
        public T Value => _value;
        public FailureBuilder<T> Swap() => new(_value);
        public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
        public override string ToString() => Build<object>().ToString();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly partial struct FailureBuilder<TError>
    {
        internal readonly TError _error;
        internal FailureBuilder(TError error) => _error = error;
        public Result<T, TError> Build<T>() => _error;

        public bool IsSuccess => false;
        public bool IsFailure => true;
        public TError Error => _error;
        public SuccessBuilder<TError> Swap() => new(_error);
        public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
        public override string ToString() => Build<object>().ToString();
    }
}

/// <summary>
/// Monad Result, Note that if TError is struct, the error value will be boxed, some boxes will be cached
/// </summary>
public readonly partial struct Result<T, TError>
    : ITaggedUnion
{
    private readonly bool _success;
    internal readonly T? _value;
    internal readonly TError? _error;

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess => _success;

    [MemberNotNullWhen(false, nameof(_value))]
    [MemberNotNullWhen(true, nameof(_error))]
    public bool IsFailure => !_success;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public TError Error => _error!;

    public T GetValueOrThrow()
    {
        if (!IsSuccess)
            ResultException.Throw<T, TError>(_error);
        return _value;
    }

    public TError GetErrorOrThrow()
    {
        if (IsSuccess)
            ResultException.Throw<T, TError>(_value);
        return _error;
    }

    public T? GetValueOrDefault() => _value;

    public TError? GetErrorOrDefault() => _error;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValueOrDefault(T? defaultValue)
        => IsSuccess ? _value : defaultValue;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public TError? GetErrorOrDefault(TError? defaultValue)
        => IsFailure ? _error : defaultValue;

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError error)
    {
        value = _value;
        error = _error;
        return IsSuccess;
    }

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsSuccess;
    }

    [MemberNotNullWhen(false, nameof(_value)), MemberNotNullWhen(true, nameof(_error))]
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        error = _error;
        return IsFailure;
    }


    private Result(bool success, T? value, TError? error)
    {
        _success = success;
        _value = value;
        _error = error;
    }

    public Result(T value)
    {
        _success = true;
        _value = value;
    }

    public Result(TError error)
    {
        _success = false;
        _error = error;
    }

    public static implicit operator Result<T, TError>(T value) => new(value);
    public static implicit operator Result<T, TError>(TError error) => new(error);
    public static implicit operator Result<T, TError>(Result.SuccessBuilder<T> builder) => new(builder._value);
    public static implicit operator Result<T, TError>(Result.FailureBuilder<TError> builder) => new(builder._error);


    public Result<TError, T> Swap() => new(!_success, _error, _value);

    public Result<TResult, TError> Cast<TResult>() => IsSuccess ? new((TResult)(object)_value) : new(_error);

    public Result<T, TNewError> CastError<TNewError>() => IsFailure ? new((TNewError)(object)_error) : new(_value);

    public Result<TResult, TNewError> Cast<TResult, TNewError>() => IsSuccess ? new((TResult)(object)_value) : new((TNewError)(object)_error);

    public override string ToString() => (IsSuccess ? _value.ToString() : _error.ToString()) ?? "";

    public string ToString(bool includeVariantInfo)
    {
        if (!includeVariantInfo) {
            return ToString();
        }

        if (IsSuccess) {
            string? str;
            if (_value is ITaggedUnion monad)
                str = monad.ToString(true);
            else
                str = _value.ToString();
            return str is null ? "Result Success" : $"Success({str})";
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

    public static bool operator ==(Result<T, TError> left, Result<T, TError> right)
    {
        return (left.IsSuccess, right.IsSuccess) switch
        {
            (true, true) => EqualityComparer<T>.Default.Equals(left.Value, right.Value),
            (false, false) => EqualityComparer<TError>.Default.Equals(left.Error, right.Error),
            _ => false,
        };
    }
    public static bool operator !=(Result<T, TError> left, Result<T, TError> right) => !(left == right);
    public bool Equals(Result<T, TError> other) => this == other;
    public override bool Equals(object? obj) => obj is Result<T, TError> other && this == other;
    public override int GetHashCode() => IsSuccess ? _value.GetHashCode() : _error.GetHashCode();
}

public sealed class ResultException : InvalidOperationException
{
    private ResultException(Type valueType, Type errorType, bool success)
        : base($"Result<{valueType.Name}, {errorType.Name}> is {(success ? "Success" : "Failure")}")
    { }

    [DoesNotReturn]
    public static void Throw<T, TError>(TError error)
#if NET9_0_OR_GREATER
        where T : allows ref struct where TError : allows ref struct
#endif
        => throw new ResultException(typeof(T), typeof(TError), false);

    [DoesNotReturn]
    public static void Throw<T, TError>(T value)
#if NET9_0_OR_GREATER
        where T : allows ref struct where TError : allows ref struct
#endif
        => throw new ResultException(typeof(T), typeof(TError), true);
}
