using System.ComponentModel;

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
                ThrowHelper.ThrowInvalidOperation($"Optional<> has no value.");
            return _value!;
        }
    }

    public readonly T? GetValueOrDefault() => _value;

    public static implicit operator Optional<T>(T value) => new(value);

    public Optional<TResult> Select<TResult>(Func<T, TResult> selector)
        => _hasValue ? new(selector(_value!)) : default;

    public Optional<TResult> SelectWrapped<TResult>(Func<T, Optional<TResult>> selector)
        => _hasValue ? selector(_value!) : default;

    public override string? ToString() => _hasValue ? _value!.ToString() : null;

    /// <remarks>
    /// This method is impl for positional pattern,
    /// You use an <c>if</c> statement to quickly check and get the value
    /// <code>
    /// if (optional is (true, var value)) {
    ///     Process(value);
    /// }
    /// _ = optional is (true, var value)
    ///     ? value : default;
    /// </code>
    /// Of course you can do similar thing in `switch` with 2 cases
    /// <c>(false, _)</c> and <c>(_, var value)</c>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out bool hasValue, out T value)
    {
        hasValue = _hasValue;
        value = _value;
    }
}
