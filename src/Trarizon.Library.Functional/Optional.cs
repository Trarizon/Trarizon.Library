//#define MONAD
//#define RESULT
//#define EITHER
//#define EXT_ENUMERABLE

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional;
public static class Optional
{
    public static OptionalNoneBuilder None => default;

    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> Create<T>(bool hasValue, T? value)
        => hasValue ? new(value!) : default;

    #region To Nullable

    public static Optional<T> OfNotNull<T>(T? value) where T : class => value is null ? default : new(value);

    public static Optional<T> OfNotNull<T>(T? value) where T : struct => value is { } v ? new(v) : default;

    public static T? ToNullable<T>(this in Optional<T> optional) where T : struct
        => optional.HasValue ? optional._value : null;

    public static T? ToNullable<T>(this Optional<T> optional) where T : class
        => optional.GetValueOrDefault();

    #endregion

    public static ref readonly T? GetValueRefOrDefaultRef<T>(this ref readonly Optional<T> optional)
        => ref optional._value;

    public static Optional<IEnumerable<T>> Collect<T>(this IEnumerable<Optional<T>> optionals)
    {
        var values = new List<T>();
        foreach (var optional in optionals) {
            if (optional.HasValue) {
                values.Add(optional._value);
            }
            else {
                return None;
            }
        }
        return values;
    }

#if RESULT

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error)
        => optional.HasValue ? new(optional._value) : new(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector)
        => optional.HasValue ? new(optional._value) : new(errorSelector());

    public static Result<Optional<T>, TError> Transpose<T, TError>(this in Optional<Result<T, TError>> optional)
    {
        if (optional._hasValue) {
            ref readonly var result = ref optional._value;
            return result.IsSuccess ? Optional.Of(result._value) : Result.Failure(result.Error);
        }
        return Result.Success(Optional<T>.None);
    }

#endif

#if EITHER

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, TRight right)
        => optional.HasValue ? new(optional._value) : new(right);

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, Func<TRight> rightSelector)
        => optional.HasValue ? new(optional._value) : new(rightSelector());

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, TLeft left)
        => optional.HasValue ? new(optional._value) : new(left);

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, Func<TLeft> leftSelector)
        => optional.HasValue ? new(optional._value) : new(leftSelector());

#endif

#if EXT_ENUMERABLE

    public static IEnumerable<TResult> WhereSelect<T, TResult>(this IEnumerable<T> source, Func<T, Optional<TResult>> selector)
    {
        if (source is T[] { Length: 0 })
            return [];
        return source.Select(selector).OfValue();
    }

    /// <summary>
    /// Filters an sequence of optional values and returns a new sequence containing those that has value
    /// </summary>
    public static IEnumerable<T> OfValue<T>(this IEnumerable<Optional<T>> source)
        => source.Where(x => x.HasValue).Select(x => x.Value);

#endif

    [DoesNotReturn]
    internal static void ThrowOptionalHasNoValue() => throw new InvalidOperationException("Optional<> has no value");
}

public readonly struct Optional<T>
#if MONAD
    : IMonad
#endif
{
    internal readonly bool _hasValue;
    internal readonly T? _value;

    public static Optional<T> None => default;

    #region Accessor

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
            Optional.ThrowOptionalHasNoValue();
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

    #endregion

    #region Creation

    internal Optional(bool hasValue, T? value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public Optional(T value) : this(true, value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Optional<T>(OptionalNoneBuilder _) => default;

    public static implicit operator Optional<T>(T value) => new(value);

    #endregion

    public static bool operator true(Optional<T> optional) => optional.HasValue;

    public static bool operator false(Optional<T> optional) => !optional.HasValue;

    public static Optional<T> operator |(Optional<T> left, Optional<T> right) => left.HasValue ? left : right;

    #region Convertor

    public Optional<TResult> Cast<TResult>() => HasValue ? new((TResult)(object)_value) : default;

    public TResult Match<TResult>(Func<T, TResult> selector, Func<TResult> noValueSelector)
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
    {
        if (HasValue && other.HasValue)
            return selector(_value, other._value);
        return default;
    }

    public Optional<(T, T2)> Zip<T2>(Optional<T2> other)
    {
        if (HasValue && other.HasValue)
            return (_value, other._value);
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
    public Optional<TResult> SelectMany<TResult>(Func<T, Optional<TResult>> selector) => Bind(selector);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Optional<TResult> SelectMany<TMid, TResult>(Func<T, Optional<TMid>> selector, Func<T, TMid, TResult> resultSelector) => Bind(selector, resultSelector);

    #endregion

    public override string ToString() => ToString(includeVariantInfo: false);

    public string ToString(bool includeVariantInfo)
    {
        if (includeVariantInfo) {
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
        else {
            return HasValue ? _value.ToString() ?? "" : "";
        }
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct OptionalNoneBuilder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Optional<T> Build<T>() => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<Optional<T>, TError> Build<T, TError>() => Result.Success(Optional<T>.None);
}
