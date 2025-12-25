using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

#if NET9_0_OR_GREATER

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
}

#endif
