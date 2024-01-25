using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Wrappers;
public static class Optional
{
    public static Optional<T> Of<T>(T value) => new(value);

    /// <summary>
    /// In fact just use <c>default</c> is ok
    /// </summary>
    public static Optional<T> None<T>() => default;

    public static Optional<T> FromNullable<T>(T? value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;

    public static T? ToNullable<T>(this Optional<T> value) where T : struct
        => value.HasValue ? value.GetValueOrDefault() : default;

    #region Conversion

    public static Result<T, TError> ToResult<T, TError>(this Optional<T> optional, TError error) where TError : class 
        => optional.HasValue ? Result.Success<T, TError>(optional._value) : Result.Failed<T, TError>(error);

    #endregion

}

public readonly struct Optional<T>(T value)
{
    internal readonly bool _hasValue = true;
    internal readonly T? _value = value;

    #region Accessor

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

    #region Linq

    public Optional<TResult> Select<TResult>(Func<T, TResult> selector)
        => HasValue ? new(selector(_value)) : default;

    public Optional<TResult> SelectWrapped<TResult>(Func<T, Optional<TResult>> selector)
        => HasValue ? selector(_value) : default;

    #endregion

    public override string? ToString() => HasValue ? _value.ToString() : null;
}
