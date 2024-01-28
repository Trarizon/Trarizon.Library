using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Wrappers;
public static class Optional
{
    public static Optional<T> Of<T>(T value) => new(value);

    /// <summary>
    /// In fact just use <c>default</c> is ok
    /// </summary>
    public static Optional<T> None<T>() => Optional<T>.None;

    public static Optional<T> FromNullable<T>(T? value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;

    public static T? ToNullable<T>(this Optional<T> value) where T : struct
        => value.HasValue ? value._value : default;

    #region Conversion

    public static Result<T, TError> ToResult<T, TError>(this Optional<T> optional, TError error) where TError : class
        => optional.HasValue ? new(optional._value) : new(error);

    public static Result<T, TError> ToResult<T, TError>(this Optional<T> optional, Func<TError> errorSelector) where TError : class
        => optional.HasValue ? new(optional._value) : new(errorSelector());

    public static Either<T, TRight> ToEitherRight<T, TRight>(this Optional<T> optional, TRight right)
        => optional.HasValue ? new(optional._value) : new(right);

    public static Either<T, TRight> ToEitherRight<T, TRight>(this Optional<T> optional, Func<TRight> rightSelector)
        => optional.HasValue ? new(optional._value) : new(rightSelector());

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this Optional<T> optional, TLeft left)
        => optional.HasValue ? new(optional._value) : new(left);

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this Optional<T> optional, Func<TLeft> leftSelector)
        => optional.HasValue ? new(optional._value) : new(leftSelector());

    #endregion

}

public readonly struct Optional<T>(T value)
{
    private readonly bool _hasValue = true;
    internal readonly T? _value = value;

    #region Accessor

    /// <summary>
    /// In fact just use <c>default</c> is ok
    /// </summary>
    public static Optional<T> None => default;

    [MemberNotNullWhen(true, nameof(_value), nameof(Value))]
    public bool HasValue => _hasValue;

    public T Value
    {
        get {
            if (!HasValue)
                ThrowHelper.ThrowInvalidOperation($"Optional<> has no value.");
            return _value;
        }
    }

    public T? GetValueOrDefault() => _value;

    [MemberNotNullWhen(true, nameof(_value), nameof(Value))]
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

    public override string? ToString() => HasValue ? _value.ToString() : null;
}
