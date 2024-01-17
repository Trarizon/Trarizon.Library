using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Wrappers;
public static class Optional
{
    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> FromNullable<T>(T? value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;

    public static T? ToNullable<T>(this Optional<T> value) where T : struct
        => value.HasValue ? value.GetValueOrDefault() : default;
}

/// <summary>
/// Optional monad indicates whether a value is exist or not, 
/// use <c>default</c> as None
/// </summary>
/// <remarks>
/// You can deconstruct this into (bool, T) to quick check
/// <code>
/// if (opt is (true, var val))
///     Process(val);
/// </code>
/// </remarks>
/// <typeparam name="T"></typeparam>
/// <param name="value"></param>
public readonly struct Optional<T>(T value)
{
    private readonly bool _hasValue = true;
    private readonly T _value = value;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_value), nameof(Value))]
    public readonly bool HasValue => _hasValue;

    public readonly T Value
    {
        get {
            if (!HasValue)
                ThrowHelper.ThrowInvalidOperation($"Optional<> has no value.");
            return _value;
        }
    }

    public readonly T? GetValueOrDefault() => _value;

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
