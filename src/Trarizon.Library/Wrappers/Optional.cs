using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Wrappers;
public static class Optional
{
    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> Of<T>(T value, Func<T, bool> predicate) => predicate(value) ? Of(value) : default;

    public static Optional<T> OfNotNull<T>(T? value) where T : class => value is null ? default : Of(value);

    public static ref readonly T? GetValueRefOrDefaultRef<T>(this ref readonly Optional<T> optional)
        => ref optional._value;

    #region Conversion

    #region Nullable

    public static Optional<T> FromNullable<T>(T? value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;

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

    #region Result

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error) where TError : class
        => optional.HasValue ? new(optional._value) : new(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector) where TError : class
        => optional.HasValue ? new(optional._value) : new(errorSelector());

    #endregion

    #endregion
}

public readonly struct Optional<T>
#if NET9_0_OR_GREATER
    where T : allows ref struct
#endif
{
    private readonly bool _hasValue = true;
    internal readonly T? _value;

    internal Optional(bool hasValue, T? value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public Optional(T value) : this(true, value) { }

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value))]
    public bool HasValue => _hasValue;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public T GetValidValue()
    {
        if (!HasValue)
            ThrowHelper.ThrowInvalidOperationException($"Optional<> has no value.");
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
