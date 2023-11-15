namespace Trarizon.Library.Wrappers;
public static class Optional
{
    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> FromNullable<T>(T? value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;

    public static T? ToNullable<T>(this Optional<T> value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;
}

public readonly struct Optional<T>(T value)
{
    private readonly bool _hasValue = true;
    private readonly T _value = value;

    public readonly bool HasValue => _hasValue;

    public readonly T Value
    {
        get {
            if (!_hasValue)
                ThrowHelper.ThrowInvalidOperation($"Option<{typeof(T).Name}> has no value.");
            return _value!;
        }
    }

    public readonly T? GetValueOrDefault() => _value;

    public static implicit operator Optional<T>(T value) => new(value);

    public Optional<TResult> Select<TResult>(Func<T, TResult> selector)
        => _hasValue ? new(selector(_value!)) : default;

    public Optional<TResult> SelectWrapped<TResult>(Func<T, Optional<TResult>> selector)
        => _hasValue ? selector(_value!) : default;

    public Optional<TResult> SelectWrapped<T2, TResult>(Func<T, Optional<T2>> selector, Func<T, T2, TResult> resultSelector)
        => _hasValue && selector(_value) is var mid && mid._hasValue
            ? new(resultSelector(_value!, mid._value))
            : default;

    public override string? ToString() => _hasValue ? _value!.ToString() : null;
}
