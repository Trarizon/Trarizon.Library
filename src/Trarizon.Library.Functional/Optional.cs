﻿#define RESULT
#define EITHER

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;
public static class Optional
{
    public static OptionalNoneBuilder None => default;

    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> Create<T>(bool hasValue, T? value)
        => hasValue ? new(value!) : default;

    #region To Nullable

    public static Optional<T> OfNotNull<T>(T? value) where T : class => value is null ? default : new(value);

    public static Optional<T> OfNotNull<T>(T? value) where T : struct => value is { } v ? new(v) : default;

    public static T? ToNullable<T>(this in Optional<T> optional) where T : struct
        => optional.HasValue ? optional._value : null;

    public static T? ToNullable<T>(this Optional<T> optional) where T : class
        => optional.GetValueOrDefault();

    #endregion

    public static ref readonly T? GetValueRefOrDefaultRef<T>(this ref readonly Optional<T> optional)
        => ref optional._value;

#if RESULT

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error) where TError : class
        => optional.HasValue ? new(optional._value) : new(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector) where TError : class
        => optional.HasValue ? new(optional._value) : new(errorSelector());

#endif

#if EITHER

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, TRight right)
        => optional.HasValue ? new(optional._value) : new(right);

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, Func<TRight> rightSelector)
        => optional.HasValue ? new(optional._value) : new(rightSelector());

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, TLeft left)
        => optional.HasValue ? new(optional._value) : new(left);

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, Func<TLeft> leftSelector)
        => optional.HasValue ? new(optional._value) : new(leftSelector());

#endif

#if NET9_0_OR_GREATER

    public static RefOptional<T> AsRef<T>(this in Optional<T> optional) => new(optional._hasValue, optional._value);

    public static Optional<T> AsOptional<T>(this in RefOptional<T> optional) => new(optional._hasValue, optional._value);

#endif

    [DoesNotReturn]
    internal static void ThrowOptionalHasNoValue() => throw new InvalidOperationException("Optional<> has no value");
}

public readonly struct Optional<T>
{
    internal readonly bool _hasValue;
    internal readonly T? _value;

    public static Optional<T> None => default;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value))]
    public bool HasValue => _hasValue;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public T GetValueOrThrow()
    {
        if (!_hasValue)
            Optional.ThrowOptionalHasNoValue();
        return _value!;
    }

    public T? GetValueOrDefault() => _value;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValueOrDefault(T? defaultValue)
        => HasValue ? _value : defaultValue;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _hasValue;
    }

    #endregion

    #region Creation

    internal Optional(bool hasValue, T? value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public Optional(T value) : this(true, value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Optional<T>(OptionalNoneBuilder _) => default;

    public static implicit operator Optional<T>(T value) => new(value);

#if NET9_0_OR_GREATER

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator RefOptional<T>(Optional<T> optional) => new(optional._hasValue, optional._value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Optional<T>(RefOptional<T> optional) => new(optional._hasValue, optional._value);

#endif

    #endregion

    #region Convertor

    public TResult Match<TResult>(Func<T, TResult> selector, Func<TResult> noValueSelector)
        => HasValue ? selector(_value) : noValueSelector();

    public void Match(Action<T>? selector, Action? noValueSelector)
    {
        if (HasValue)
            selector?.Invoke(_value);
        else
            noValueSelector?.Invoke();
    }

    public void MatchValue(Action<T> selector)
    {
        if (_hasValue) selector(_value!);
    }

    public void MatchNone(Action selector)
    {
        if (!_hasValue) selector();
    }

    public Optional<TResult> Select<TResult>(Func<T, TResult> selector)
        => HasValue ? new(selector(_value)) : default;

    public Optional<TResult> Bind<TResult>(Func<T, Optional<TResult>> selector)
        => HasValue ? selector(_value) : default;

    public Optional<TResult> Bind<TMid, TResult>(Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
    {
        if (HasValue) {
            var mid = selector(_value);
            if (mid.HasValue)
                return resultSelector(_value, mid._value);
        }
        return default;
    }

    public Optional<TResult> Zip<T2, TResult>(Optional<T2> other, Func<T, T2, TResult> selector)
    {
        if (HasValue && other.HasValue)
            return selector(_value, other._value);
        return default;
    }

    public Optional<T> Where(Func<T, bool> predicate)
    {
        if (HasValue && predicate(_value))
            return _value;
        else
            return default;
    }

    // The method is declared for linq expression
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> selector) => Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<TResult> SelectMany<TMid, TResult>(Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector) => Bind(selector, resultSelector);

    #endregion

    public override string ToString() => HasValue ? _value.ToString() ?? string.Empty : string.Empty;
}

#if NET9_0_OR_GREATER

public readonly ref struct RefOptional<T> where T : allows ref struct
{
    internal readonly bool _hasValue;
    internal readonly T? _value;

    public static RefOptional<T> None => default;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool HasValue => _hasValue;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public T GetValueOrThrow()
    {
        if (_hasValue)
            Optional.ThrowOptionalHasNoValue();
        return _value!;
    }

    public T? GetValueOrDefault() => _value;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValueOrDefault(T? defaultValue)
        => _hasValue ? _value : defaultValue;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _hasValue;
    }

    internal RefOptional(bool hasValue, T? value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public RefOptional(T value) : this(true, value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator RefOptional<T>(OptionalNoneBuilder _) => default;

    public static implicit operator RefOptional<T>(T value) => new(value);

    public TResult Match<TResult>(Func<T, TResult> selector, Func<TResult> noneSelector) where TResult : allows ref struct
        => _hasValue ? selector(_value!) : noneSelector();

    public void Match(Action<T>? selector, Action? noneSelector)
    {
        if (_hasValue)
            selector?.Invoke(_value!);
        else
            noneSelector?.Invoke();
    }

    public void MatchValue(Action<T> selector)
    {
        if (_hasValue) selector(_value!);
    }

    public void MatchNone(Action selector)
    {
        if (!_hasValue) selector();
    }

    public RefOptional<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : allows ref struct
        => _hasValue ? new(selector(_value!)) : default;

    public RefOptional<TResult> Bind<TResult>(Func<T, RefOptional<TResult>> selector) where TResult : allows ref struct
        => _hasValue ? selector(_value!) : default;

    public RefOptional<TResult> Bind<TMid, TResult>(Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector) where TMid : allows ref struct where TResult : allows ref struct
    {
        if (_hasValue) {
            var mid = selector(_value!);
            if (mid._hasValue) {
                return resultSelector(_value!, mid._value!);
            }
        }
        return default;
    }

    public RefOptional<TResult> Zip<T2, TResult>(RefOptional<T2> other, Func<T, T2, TResult> selector) where T2 : allows ref struct where TResult : allows ref struct
    {
        if (_hasValue && other._hasValue) {
            return selector(_value!, other._value!);
        }
        return default;
    }

    public RefOptional<T> Where(Func<T, bool> predicate)
    {
        if (_hasValue && predicate(_value!))
            return _value!;
        else
            return default;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RefOptional<TResult> SelectMany<TResult>(Func<T, RefOptional<TResult>> selector) where TResult : allows ref struct => Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RefOptional<TResult> SelectMany<TMid, TResult>(Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector) where TMid : allows ref struct where TResult : allows ref struct => Bind(selector, resultSelector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString() => $"RefOptional<{typeof(T).Name}>";
}

#endif

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct OptionalNoneBuilder
{
    public Optional<T> Build<T>() => default;
}