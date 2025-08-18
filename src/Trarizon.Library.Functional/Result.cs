//#define OPTIONAL
//#define EITHER
//#define EXT_ENUMERABLE

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;
public static partial class Result
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

    public static Result<IEnumerable<T>, TError> Collect<T, TError>(this IEnumerable<Result<T, TError>> results)
    {
        var values = new List<T>();
        foreach (var result in results) {
            if (result.IsSuccess) {
                values.Add(result._value);
            }
            else {
                return result.Error;
            }
        }
        return values;
    }

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

#if EXT_ENUMERABLE

    public static IEnumerable<T> OfValue<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsSuccess).Select(x => x.Value);

    public static IEnumerable<TError> OfError<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsError).Select(x => x.Error!);

#endif

    [DoesNotReturn]
    internal static void ThrowResultIsError<TError>(TError error) => throw new ResultErrorException<TError>(error);

    internal sealed class ValueTypeBox<T>(T value)
    {
        public readonly T Value = value;
    }

    internal static class ErrorHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Wrap<T>(T? value)
        {
            if (typeof(T).IsValueType) {
                if (value is null) {
                    return CacheDefault<T>.Value;
                }
                if (typeof(T) == typeof(bool)) {
                    var val = Unsafe.As<T, bool>(ref value!);
                    return val ? CacheBoolean.True : CacheBoolean.False;
                }
#if NETSTANDARD2_0
                if (typeof(T) == typeof(int)) {
                    return Cache32.Get(Unsafe.As<T, int>(ref value!));
                }
#else
                if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                    if (Unsafe.SizeOf<T>() == sizeof(byte) && Unsafe.As<T, byte>(ref value) == default(byte))
                        return CacheDefault<byte>.Value;
                    if (Unsafe.SizeOf<T>() == sizeof(short) && Unsafe.As<T, short>(ref value) == default(short))
                        return CacheDefault<short>.Value;
                    if (Unsafe.SizeOf<T>() == sizeof(int))
                        return Cache32.Get(Unsafe.As<T, int>(ref value));
                    if (Unsafe.SizeOf<T>() == sizeof(long) && Unsafe.As<T, long>(ref value) == default(long))
                        return CacheDefault<long>.Value;
                }
#endif
                return new ValueTypeBox<T>(value!);
            }
            else {
                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Get<T>(object? value)
        {
            Debug.Assert(IsValidErrorObject<T>(value));

            if (typeof(T).IsValueType) {
                var box = Unsafe.As<ValueTypeBox<T>>(value);
                if (box is null)
                    return default;
                return box.Value;
            }
            else {
                Debug.Assert(value is T);
                return Unsafe.As<object?, T?>(ref value);
            }
        }

        public static string ToStringIncludeVariantInfo<T>(ValueTypeBox<T> value)
        {
            if (value.Value is IMonad monad)
                return monad.ToString(true);
            return value.ToString() ?? "";
        }

        public static bool IsValidErrorObject<T>(object? obj)
        {
            if (obj is null)
                return true;
            if (!typeof(T).IsValueType)
                return true;

#if NETSTANDARD2_0
            return obj is ValueTypeBox<T>;
#else
            if (obj is ValueTypeBox<T>)
                return true;
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                if (Unsafe.SizeOf<T>() == sizeof(byte) && obj is ValueTypeBox<byte>)
                    return true;
                if (Unsafe.SizeOf<T>() == sizeof(short) && obj is ValueTypeBox<short>)
                    return true;
                if (Unsafe.SizeOf<T>() == sizeof(int) && obj is ValueTypeBox<int>)
                    return true;
                if (Unsafe.SizeOf<T>() == sizeof(long) && obj is ValueTypeBox<long>)
                    return true;
            }
            return false;
#endif
        }

        private static class CacheDefault<T>
        {
            public static readonly ValueTypeBox<T> Value = new ValueTypeBox<T>(default!);
        }

        private static class CacheBoolean
        {
            public static readonly ValueTypeBox<bool> True = new ValueTypeBox<bool>(true);
            public static readonly ValueTypeBox<bool> False = new ValueTypeBox<bool>(false);
        }

        private static class Cache32
        {
            private const int Min = -16;
            private const int Max = 16;

#if NET8_0_OR_GREATER
            private static Arr _cache;
#else
            private static readonly ValueTypeBox<int>?[] _cache = new ValueTypeBox<int>[Max - Min];
#endif

            public static ValueTypeBox<int> Get(int value)
            {
                if (value < Min || value >= Max)
                    return new ValueTypeBox<int>(value);
                return _cache[value - Min] ??= new ValueTypeBox<int>(value);
            }

#if NET8_0_OR_GREATER

            [InlineArray(Max - Min)] struct Arr { ValueTypeBox<int> _; }

#endif
        }
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

    public TError? Error => Result.ErrorHelper.Get<TError>(_error);

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
        Debug.Assert(Result.ErrorHelper.IsValidErrorObject<TError>(error));
        _error = error;
    }

    private Result(T? value, TError? error)
    {
        _value = value;
        _error = Result.ErrorHelper.Wrap<TError>(error);
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
                    str = Result.ErrorHelper.ToStringIncludeVariantInfo(Unsafe.As<Result.ValueTypeBox<TError>>(_error));
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
