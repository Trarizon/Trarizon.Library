using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

public static class Optional
{
    public static NoneBuilder None => default;

    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> Create<T>(bool hasValue, T value)
        => hasValue ? new(value) : default;

    public static Optional<T> OfNotNull<T>(T? value) where T : class => value is null ? default : new(value);

    public static Optional<T> OfNotNull<T>(T? value) where T : struct => value is { } v ? new(v) : default;

    public static Optional<T> OfPredicate<T>(T value, Func<T, bool> predicate) => predicate(value) ? new(value) : default;

    public static ref readonly T? GetValueRefOrDefaultRef<T>(this ref readonly Optional<T> optional)
        => ref optional._value;

    public static Optional<T> OfNotNull<T>(this Optional<T?> value) where T : class
        => value.HasValue && value._value is { } v ? new(v) : default;

    public static Optional<T> OfNotNull<T>(this Optional<T?> value) where T : struct
        => value.HasValue && value._value is { } v ? new(v) : default;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct NoneBuilder
    {
        public Optional<T> Build<T>() => default;

        public bool HasValue => false;
        public static bool operator true(NoneBuilder _) => false;
        public static bool operator false(NoneBuilder _) => true;
        public Optional<T> Or<T>(Optional<T> other) => other;
        public Optional<T> Or<T>(Func<Optional<T>> otherSelector) => otherSelector();
    }
}

public readonly partial struct Optional<T>
#if MONAD
    : IMonad
#endif
{
    internal readonly bool _hasValue;
    internal readonly T? _value;

    public static Optional<T> None => default;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool HasValue => _hasValue;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public T GetValueOrThrow()
    {
        if (!_hasValue)
            OptionalNoValueException.Throw<T>();
        return _value!;
    }

    public T? GetValueOrDefault() => _value;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValueOrDefault(T? defaultValue)
        => HasValue ? _value : defaultValue;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _hasValue;
    }

    internal Optional(bool hasValue, T? value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public Optional(T value) : this(true, value) { }

    public static implicit operator Optional<T>(Optional.NoneBuilder _) => default;
    public static implicit operator Optional<T>(T value) => new(value);

    public static bool operator true(Optional<T> optional) => optional.HasValue;
    public static bool operator false(Optional<T> optional) => !optional.HasValue;
    public static Optional<T> operator |(Optional<T> left, Optional<T> right) => left.HasValue ? left : right;

    public Optional<TResult> Cast<TResult>() => HasValue ? new((TResult)(object)_value) : default;

    public TResult Match<TResult>(Func<T, TResult> selector, Func<TResult> noValueSelector)
#if NET9_0_OR_GREATER
        where TResult : allows ref struct
#endif
        => HasValue ? selector(_value) : noValueSelector();

    public void Match(Action<T>? selector, Action? noValueSelector)
    {
        if (HasValue)
            selector?.Invoke(_value);
        else
            noValueSelector?.Invoke();
    }

    public void MatchValue(Action<T> selector)
    {
        if (_hasValue) selector(_value!);
    }

    public void MatchNone(Action selector)
    {
        if (!_hasValue) selector();
    }

    public Optional<T> Or(Optional<T> other) => _hasValue ? this : other;

    public Optional<T> Or(Func<Optional<T>> otherSelector) => _hasValue ? this : otherSelector();

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
        => HasValue && other.HasValue ? new(selector(_value, other._value)) : default;

    public Optional<(T, T2)> Zip<T2>(Optional<T2> other)
        => HasValue && other.HasValue ? new((_value, other._value)) : default;

    public Optional<T> Where(Func<T, bool> predicate)
        => HasValue && predicate(_value) ? new(_value) : default;

    // The method is declared for linq expression
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> selector) => Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<TResult> SelectMany<TMid, TResult>(Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector) => Bind(selector, resultSelector);

    public override string ToString() => HasValue ? _value.ToString() ?? "" : "";

    public string ToString(bool includeVariantInfo)
    {
        if (!includeVariantInfo) {
            return ToString();
        }

        if (HasValue) {
            string? str;
#if MONAD
            if (_value is IMonad monad)
                str = monad.ToString(true);
            else
#endif
                str = _value.ToString();
            return str is null ? "Optional Value" : $"Value({str})";
        }
        else {
            return "Optional None";
        }
    }

    public static bool operator ==(Optional<T> left, Optional<T> right)
    {
        return (left.HasValue, right.HasValue) switch
        {
            (true, true) => EqualityComparer<T>.Default.Equals(left.Value, right.Value),
            (false, false) => true,
            _ => false,
        };
    }
    public static bool operator !=(Optional<T> left, Optional<T> right) => !(left == right);
    public bool Equals(Optional<T> other) => this == other;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Optional<T> other && this == other;
    public override int GetHashCode() => HasValue ? _value.GetHashCode() : 0;
}

public sealed class OptionalNoValueException : Exception
{
    private OptionalNoValueException(Type type)
        : base($"Optional<{type.Name}> has no value")
    { }

    [DoesNotReturn]
    public static void Throw<T>()
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        => throw new OptionalNoValueException(typeof(T));
}
