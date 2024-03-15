using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;

namespace Trarizon.Library.Wrappers;
public static class Result
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Success<T, TError>(T value) where TError : class
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TError> Failed<T, TError>(TError error) where TError : class
        => new(error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfFailed<T, TException>(this in Result<T, TException> result) where TException : Exception
    {
        if (!result.Success)
            ThrowHelper.Throw(result._error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T? GetValueRefOrDefaultRef<T, TError>(this in Result<T, TError> result) where TError : class
        => ref result._value;

    #region Conversion

    public static Optional<T> ToOptional<T, TError>(this in Result<T, TError> result) where TError : class
        => result.Success ? Optional.Of(result._value) : default;

    public static Either<T, TError> AsEitherLeft<T, TError>(this in Result<T, TError> result) where TError : class
        => result.Success ? new(result._value) : new(result._error);

    public static Either<TError, T> AsEitherRight<T, TError>(this in Result<T, TError> result) where TError : class
        => result.Failed ? new(result._error) : new(result._value);

    #endregion
}

public readonly struct Result<T, TError> where TError : class
{
    [FriendAccess(typeof(Result))]
    internal readonly T? _value;
    [FriendAccess(typeof(Result))]
    internal readonly TError? _error;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool Success => _error is null;

    [MemberNotNullWhen(false, nameof(_value))]
    [MemberNotNullWhen(true, nameof(_error))]
    public bool Failed => !Success;

    public T Value => _value!;

    public NotNull<TError> Error => _error;

    public T GetValidValue()
    {
        if (Failed)
            ThrowHelper.ThrowInvalidOperation($"Result<> failed.");
        return _value;
    }

    public T? GetValueOrDefault() => _value;
    public TError? GetErrorOrDefault() => _error;

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError error)
    {
        value = _value;
        error = _error;
        return Success;
    }

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
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

    public override string ToString() => (Success ? _value.ToString() : _error.ToString()) ?? string.Empty;
}
