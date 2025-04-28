using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional.Monads;
public static class Optional
{
    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> Of<T>(T value, Func<T, bool> predicate) => predicate(value) ? new(value) : default;

    public static Optional<TResult> Of<T, TResult>(T value, Func<T, bool> predicate, Func<T, TResult> valueSelector)
        => predicate(value) ? new(valueSelector(value)) : default;

    public static Optional<T> OfNotNull<T>(T? value) where T : class => value is null ? default : new(value);

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

    [DoesNotReturn]
    internal static void ThrowOptionalHasNoValue() => throw new InvalidOperationException("Optional<> has no value");
}

public readonly struct Optional<T>
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
            Optional.ThrowOptionalHasNoValue();
        return _value;
    }

    public T? GetValueOrDefault() => _value;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _hasValue;
    }

    #endregion

    #region Creation

    public static implicit operator Optional<T>(T value) => new(value);

    #endregion

    #region Convertor

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
    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> selector)
        => Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<TResult> SelectMany<TMid, TResult>(Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        => Bind(selector, resultSelector);

    #endregion

    public override string ToString() => HasValue ? _value.ToString() ?? string.Empty : string.Empty;
}
