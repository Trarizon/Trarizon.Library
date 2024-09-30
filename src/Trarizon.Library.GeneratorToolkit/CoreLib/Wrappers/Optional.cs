using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.CoreLib.Wrappers;
public static class Optional
{
    #region Instance Methods Ext

    public static bool TryGetValue<T>(this Optional<T> optional, [MaybeNullWhen(false)] out T value)
    {
        if (optional.HasValue)
        {
            value = optional.Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static Optional<TResult> Select<T, TResult>(this Optional<T> optional, Func<T, TResult> selector)
    => optional.HasValue ? new(selector(optional.Value)) : default;

    public static Optional<TResult> SelectWrapped<T, TResult>(this Optional<T> optional, Func<T, Optional<TResult>> selector)
        => optional.HasValue ? selector(optional.Value) : default;

    #endregion

    public static Optional<T> Of<T>(T value) => new(value);

    public static Optional<T> Of<T>(T value, Func<T, bool> predicate) => predicate(value) ? Of(value) : default;

    public static Optional<T> OfNotNull<T>(T? value) where T : class => value is null ? default : Of(value);

    #region Conversion

    #region Nullable

    public static Optional<T> FromNullable<T>(T? value) where T : struct
        => value.HasValue ? value.GetValueOrDefault()! : default;

    public static T? ToNullable<T>(this in Optional<T> optional) where T : struct
        => optional.HasValue ? optional.Value : null;

    #endregion

    #region Either

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, TRight right)
        => optional.HasValue ? new(optional.Value) : new(right);

    public static Either<T, TRight> ToEitherRight<T, TRight>(this in Optional<T> optional, Func<TRight> rightSelector)
        => optional.HasValue ? new(optional.Value) : new(rightSelector());

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, TLeft left)
        => optional.HasValue ? new(optional.Value) : new(left);

    public static Either<TLeft, T> ToEitherLeft<T, TLeft>(this in Optional<T> optional, Func<TLeft> leftSelector)
        => optional.HasValue ? new(optional.Value) : new(leftSelector());

    #endregion

    #region Result

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error) where TError : class
        => optional.HasValue ? new(optional.Value) : new(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector) where TError : class
        => optional.HasValue ? new(optional.Value) : new(errorSelector());

    #endregion

    #endregion
}
