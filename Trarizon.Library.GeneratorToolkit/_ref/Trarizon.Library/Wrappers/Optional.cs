using Microsoft.CodeAnalysis;
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
        => value.HasValue ? value.Value : default;

    public static Optional<T> Unwrap<T>(this Optional<Optional<T>> optional)
        => optional.HasValue ? optional.Value : default;

    #region Conversion

    public static Result<T, TError> ToResult<T, TError>(this Optional<T> optional, TError error) where TError : class
        => optional.HasValue ? new(optional.Value) : new(error);

    public static Result<T, TError> ToResult<T, TError>(this Optional<T> optional, Func<TError> errorSelector) where TError : class
        => optional.HasValue ? new(optional.Value) : new(errorSelector());

    public static Either<T, TRight> ToEitherRight<T, TRight>(this Optional<T> optional, TRight right)
        => optional.HasValue ? new(optional.Value) : new(right);

    public static Either<T, TRight> ToEitherRight<T, TRight>(this Optional<T> optional, Func<TRight> rightSelector)
        => optional.HasValue ? new(optional.Value) : new(rightSelector());

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this Optional<T> optional, TLeft left)
        => optional.HasValue ? new(optional.Value) : new(left);

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this Optional<T> optional, Func<TLeft> leftSelector)
        => optional.HasValue ? new(optional.Value) : new(leftSelector());

    #endregion

    public static Optional<TResult> Select<T, TResult>(this Optional<T> optional, Func<T, TResult> selector)
        => optional.HasValue ? new(selector(optional.Value)) : default;

    public static Optional<TResult> SelectWrapped<T, TResult>(this Optional<T> optional, Func<T, Optional<TResult>> selector)
        => optional.HasValue ? selector(optional.Value) : default;

    public static bool TryGetValue<T>(this Optional<T> optional, [MaybeNullWhen(false)] out T value)
    {
        value = optional.Value;
        return optional.HasValue;
    }
}
