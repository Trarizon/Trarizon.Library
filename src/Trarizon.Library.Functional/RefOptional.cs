using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

#if OPTIONAL && NET9_0_OR_GREATER

public static partial class RefOptional
{
    public static RefOptional<T> Build<T>(this Optional.NoneBuilder _) where T : allows ref struct => default;

    extension(Optional)
    {
        public static RefOptional<T> Of<T>(T value) where T : allows ref struct => new(value);

        public static RefOptional<T> Create<T>(bool hasValue, T value) where T : allows ref struct
            => hasValue ? new(value) : default;

        public static RefOptional<T> OfPredicate<T>(T value, Func<T, bool> predicate) where T : allows ref struct
            => predicate(value) ? new(value) : default;
    }

    public static ref readonly T? GetValueRefOrDefaultRef<T>(this ref readonly RefOptional<T> optional) where T : allows ref struct
        => ref optional._value;

    public static Optional<T> AsDeref<T>(this RefOptional<T> self) => self;
    public static RefOptional<T> AsRef<T>(this Optional<T> self) => self;

    public static RefOptional<TResult> Select<T, TResult>(this Optional<T> self, Func<T, TResult> selector) where TResult : allows ref struct
        => self.HasValue ? new(selector(self.Value)) : default;
    public static RefOptional<TResult> Select<T, TResult>(this RefOptional<T> self, Func<T, TResult> selector) where T : allows ref struct where TResult : allows ref struct
        => self.HasValue ? new(selector(self.Value)) : default;

    public static RefOptional<TResult> Bind<T, TResult>(this Optional<T> self, Func<T, RefOptional<TResult>> selector) where TResult : allows ref struct
        => self.HasValue ? selector(self.Value) : default;
    public static RefOptional<TResult> Bind<T, TResult>(this RefOptional<T> self, Func<T, RefOptional<TResult>> selector) where T : allows ref struct where TResult : allows ref struct
        => self.HasValue ? selector(self.Value) : default;
}

public readonly ref struct RefOptional<T>
    where T : allows ref struct
{
    internal readonly bool _hasValue;
    internal readonly T? _value;

    public static RefOptional<T> None => default;

    [MemberNotNullWhen(true, nameof(_value))]
    public bool HasValue => _hasValue;

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

    internal RefOptional(bool hasValue, T? value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public RefOptional(T value) : this(true, value) { }

    public static implicit operator RefOptional<T>(T value) => new(value);
    public static implicit operator RefOptional<T>(Optional.NoneBuilder _) => default;

    public static bool operator true(RefOptional<T> optional) => optional.HasValue;
    public static bool operator false(RefOptional<T> optional) => !optional.HasValue;
    public static RefOptional<T> operator |(RefOptional<T> left, RefOptional<T> right) => left.HasValue ? left : right;

    public TResult Match<TResult>(Func<T, TResult> selector, Func<TResult> noneSelector)
        where TResult : allows ref struct
        => HasValue ? selector(_value) : noneSelector();

    public void Match(Action<T>? selector, Action? noneSelector)
    {
        if (HasValue)
            selector?.Invoke(_value);
        else
            noneSelector?.Invoke();
    }

    public void MatchValue(Action<T> selector)
    {
        if (HasValue) selector(_value);
    }

    public void MatchNone(Action selector)
    {
        if (!HasValue) selector();
    }

    public RefOptional<T> Or(RefOptional<T> other) => HasValue ? this : other;
    public RefOptional<T> Or(Func<RefOptional<T>> otherSelector) => HasValue ? this : otherSelector();

    public Optional<TResult> Select<TResult>(Func<T, TResult> selector)
        => HasValue ? Optional.Of(selector(_value)) : Optional.None;

    public Optional<TResult> Bind<TResult>(Func<T, Optional<TResult>> selector)
        => HasValue ? selector(_value) : default;

    public Optional<TResult> Bind<TMid, TResult>(Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
    {
        if (HasValue) {
            var mid = selector(_value);
            if (mid.HasValue)
                return new(resultSelector(_value, mid._value));
        }
        return default;
    }

    public Optional<TResult> Bind<TMid, TResult>(Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct
    {
        if (HasValue) {
            var mid = selector(_value);
            if (mid.HasValue)
                return new(resultSelector(_value, mid._value));
        }
        return default;
    }

    public RefOptional<TResult> Zip<T2, TResult>(RefOptional<T2> other, Func<T, T2, TResult> selector)
        where T2 : allows ref struct where TResult : allows ref struct
        => HasValue && other.HasValue ? new(selector(_value, other._value)) : default;

    public RefOptional<T> Where(Func<T, bool> predicate)
        => HasValue && predicate(_value) ? this : default;

    // The method is declared for linq expression
    [EditorBrowsable(EditorBrowsableState.Never)]
    public RefOptional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> selector) => Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RefOptional<TResult> SelectMany<TMid, TResult>(Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct
        => Bind(selector, resultSelector);

#pragma warning disable CS0809

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Not supported for ref struct")]
    public override string ToString() => throw new NotSupportedException();

#pragma warning restore CS0809
}

partial struct Optional<T>
{
    public static implicit operator RefOptional<T>(Optional<T> optional) => new(optional._hasValue, optional._value);
    public static implicit operator Optional<T>(RefOptional<T> optional) => new(optional._hasValue, optional._value);

    public Optional<TResult> Bind<TMid, TResult>(Func<T, RefOptional<TMid>> selector, Func<T, TMid, TResult> resultSelector)
        where TMid : allows ref struct
    {
        if (HasValue) {
            var mid = selector(_value);
            if (mid.HasValue)
                return resultSelector(_value, mid.Value);
        }
        return default;
    }
}

#endif
