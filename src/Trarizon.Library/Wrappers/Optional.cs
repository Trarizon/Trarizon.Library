using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.CodeAnalysis.MemberAccess;

namespace Trarizon.Library.Wrappers;
public static class Optional
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> Of<T>(T value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> Of<T>(T value, Func<T, bool> predicate) => predicate(value) ? Of(value) : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> OfNotNull<T>(T? value) where T : class => NotNull.As(value).ToOptional();

    /// <summary>
    /// In fact just use <c>default</c> is ok
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> None<T>() => Optional<T>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T? GetValueRefOrDefaultRef<T>(this in Optional<T> optional)
        => ref optional._value;

    #region Conversion

    #region Nullable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> FromNullable<T>(T? value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ToNullable<T>(this in Optional<T> optional) where T : struct
        => optional.HasValue ? optional._value : null;

    #endregion

    #region Either

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, TRight right)
        => optional.HasValue ? new(optional._value) : new(right);

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, Func<TRight> rightSelector)
        => optional.HasValue ? new(optional._value) : new(rightSelector());

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, TLeft left)
        => optional.HasValue ? new(optional._value) : new(left);

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, Func<TLeft> leftSelector)
        => optional.HasValue ? new(optional._value) : new(leftSelector());

    #endregion

    #region NotNull

    public static NotNull<T> ToNotNull<T>(this in Optional<T> optional) where T : class
        => optional.HasValue ? NotNull.Of(optional._value) : default;

    #endregion

    #region Result

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error) where TError : class
        => optional.HasValue ? new(optional._value) : new(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector) where TError : class
        => optional.HasValue ? new(optional._value) : new(errorSelector());

    #endregion

    #endregion
}

public readonly struct Optional<T>(T value) : IOptional<T>
{
    private readonly bool _hasValue = true;
    [FriendAccess(typeof(Optional))]
    internal readonly T? _value = value;

    #region Accessor

    /// <summary>
    /// In fact just use <c>default</c> is ok
    /// </summary>
    public static Optional<T> None => default;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool HasValue => _hasValue;

    public T Value => _value!;

    public T GetValidValue()
    {
        if (!HasValue)
            ThrowHelper.ThrowInvalidOperation($"Optional<> has no value.");
        return _value;
    }

    public T? GetValueOrDefault() => _value;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _hasValue;
    }

    #endregion

    #region Creator

    public static implicit operator Optional<T>(T value) => new(value);

    #endregion

    #region Convertor

    public Optional<TResult> Select<TResult>(Func<T, TResult> selector)
        => HasValue ? new(selector(_value)) : default;

    public Optional<TResult> SelectWrapped<TResult>(Func<T, Optional<TResult>> selector)
        => HasValue ? selector(_value) : default;

    #endregion

    public override string ToString() => HasValue ? _value.ToString() ?? string.Empty : string.Empty;
}

public interface IOptional<T>
{
    bool HasValue { get; }
    T Value { get; }
    T? GetValueOrDefault();
    bool TryGetValue([MaybeNullWhen(false)] out T? Value);
}
