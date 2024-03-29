﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;

namespace Trarizon.Library.Wrappers;
public static class NotNull
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NotNull<T> Of<T>(T value) where T : class => Unsafe.As<T, NotNull<T>>(ref value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NotNull<T> Null<T>() where T : class => NotNull<T>.Null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NotNull<T> As<T>(T? value) where T : class => Unsafe.As<T?, NotNull<T>>(ref value);

    #region Conversion

    #region Optional

    public static Optional<T> ToOptional<T>(this NotNull<T> notNull) where T : class
        => notNull.HasValue ? Optional.Of(notNull.Value) : default;

    #endregion

    #endregion
}

/// <summary>
/// This wraps a reference type for nullable check.
/// Use this on those senarios where you want to use a <see langword="null"/>
/// means invalid return.
/// </summary>
public readonly struct NotNull<T>(T value) : IOptional<T> where T : class
{
    [FriendAccess(typeof(NotNull))]
    internal readonly T? _value = value;

    #region Accessor

    public static NotNull<T> Null => default;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool HasValue => _value is not null;

    public T Value => _value!;

    public T GetValidValue()
    {
        if (!HasValue)
            ThrowHelper.ThrowInvalidOperation($"Value in NotNull<T> is null");
        return _value;
    }

    public T? GetValueOrNull() => _value;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = _value;
        return HasValue;
    }

    #endregion

    #region Creator

    public static implicit operator NotNull<T>(T? value) => NotNull.As(value);

    public static implicit operator T?(NotNull<T> notNull) => notNull._value;

    #endregion

    #region Convertor

    public NotNull<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : class
        => HasValue ? NotNull.Of(selector(_value)) : default;

    public NotNull<TResult> SelectWrapped<TResult>(Func<T, NotNull<TResult>> selector) where TResult : class
        => HasValue ? selector(_value) : default;

    #endregion

    public override string ToString() => HasValue ? _value.ToString() ?? string.Empty : string.Empty;

    #region IOptional

    T? IOptional<T>.GetValueOrDefault() => GetValueOrNull();

    #endregion
}
