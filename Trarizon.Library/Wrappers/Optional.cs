using System.ComponentModel;
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

    public static implicit operator Optional<T>(T value) => new(value);

    public Optional<TResult> Select<TResult>(Func<T, TResult> selector)
        => HasValue ? new(selector(_value)) : default;

    public Optional<TResult> SelectWrapped<TResult>(Func<T, Optional<TResult>> selector)
        => HasValue ? selector(_value) : default;

    public override string? ToString() => HasValue ? _value.ToString() : null;

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
