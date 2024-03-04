using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Wrappers;
public static class Either
{
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft leftValue)
        => new(leftValue);

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight rightValue)
        => new(rightValue);

    #region Conversion

    public static Optional<TLeft> ToOptionalLeft<TLeft, TRight>(this Either<TLeft, TRight> either)
        => either.IsLeft ? Optional.Of(either._left) : default;

    public static Optional<TRight> ToOptionalRight<TLeft, TRight>(this Either<TLeft, TRight> either)
        => either.IsRight ? Optional.Of(either._right) : default;

    public static Result<TLeft, TError> ToResultLeft<TLeft, TRight, TError>(this Either<TLeft, TRight> either, TError error) where TError : class
        => either.IsLeft ? new(either._left) : new(error);

    public static Result<TLeft, TError> ToResultLeft<TLeft, TRight, TError>(this Either<TLeft, TRight> either, Func<TRight, TError> errorSelector) where TError : class
        => either.IsLeft ? new(either._left) : new(errorSelector(either._right));

    public static Result<TRight, TError> ToResultRight<TLeft, TRight, TError>(this Either<TLeft, TRight> either, TError error) where TError : class
        => either.IsRight ? new(either._right) : new(error);

    public static Result<TRight, TError> ToResultRight<TLeft, TRight, TError>(this Either<TLeft, TRight> either, Func<TLeft, TError> errorSelector) where TError : class
        => either.IsRight ? new(either._right) : new(errorSelector(either._left));

    public static Result<TLeft, TRight> AsResultLeft<TLeft, TRight>(this Either<TLeft, TRight> either) where TRight : class
        => either.IsLeft ? new(either._left) : new(either._right);

    public static Result<TRight, TLeft> AsResultRight<TLeft, TRight>(this Either<TLeft, TRight> either) where TLeft : class
        => either.IsRight ? new(either._right) : new(either._left);

    #endregion
}

public readonly struct Either<TLeft, TRight>
{
    private readonly bool _isLeft;
    internal readonly TLeft? _left;
    internal readonly TRight? _right;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_left), nameof(LeftValue))]
    [MemberNotNullWhen(false, nameof(_right), nameof(RightValue))]
    public bool IsLeft => _isLeft;

    [MemberNotNullWhen(false, nameof(_left), nameof(LeftValue))]
    [MemberNotNullWhen(true, nameof(_right), nameof(RightValue))]
    public bool IsRight => !_isLeft;

    public TLeft LeftValue
    {
        get
        {
            return _left!;
        }
    }

    public TRight RightValue
    {
        get
        {
            return _right!;
        }
    }

    public readonly TLeft? GetLeftValueOrDefault() => _left;
    public readonly TRight? GetRightValueOrDefault() => _right;

    [MemberNotNullWhen(true, nameof(_left), nameof(LeftValue))]
    [MemberNotNullWhen(false, nameof(_right), nameof(RightValue))]
    public bool TryGetLeftValue([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
    {
        left = _left;
        right = _right;
        return IsLeft;
    }

    [MemberNotNullWhen(true, nameof(_left), nameof(LeftValue))]
    [MemberNotNullWhen(false, nameof(_right), nameof(RightValue))]
    public bool TryGetLeftValue([MaybeNullWhen(false)] out TLeft left)
    {
        left = _left;
        return IsLeft;
    }

    [MemberNotNullWhen(false, nameof(_left), nameof(LeftValue))]
    [MemberNotNullWhen(true, nameof(_right), nameof(RightValue))]
    public bool TryGetRightValue([MaybeNullWhen(false)] out TRight right, [MaybeNullWhen(true)] out TLeft left)
        => !TryGetLeftValue(out left, out right);

    [MemberNotNullWhen(false, nameof(_left), nameof(LeftValue))]
    [MemberNotNullWhen(true, nameof(_right), nameof(RightValue))]
    public bool TryGetRightValue([MaybeNullWhen(false)] out TRight right)
    {
        right = _right;
        return IsRight;
    }

    #endregion

    #region Creator

    private Either(bool isLeft, TLeft? left, TRight? right)
    {
        _isLeft = isLeft;
        _left = left;
        _right = right;
    }

    public Either(TLeft left) : this(true, left, default) { }

    public Either(TRight right) : this(false, default, right) { }

    public static implicit operator Either<TLeft, TRight>(TLeft left) => new(left);
    public static implicit operator Either<TLeft, TRight>(TRight right) => new(right);

    #endregion

    #region Convertor

    public Either<TRight, TLeft> Swap()
        => new(!_isLeft, _right, _left);

    public Either<TNewLeft, TRight> SelectLeft<TNewLeft>(Func<TLeft, TNewLeft> selector)
        => IsLeft ? new(selector(_left)) : new(_right);

    public Either<TNewLeft, TRight> SelectLeftWrapped<TNewLeft>(Func<TLeft, Either<TNewLeft, TRight>> selector)
        => IsLeft ? selector(_left) : new(_right);

    public Either<TLeft, TNewRight> SelectRight<TNewRight>(Func<TRight, TNewRight> selector)
        => IsRight ? new(selector(_right)) : new(_left);

    public Either<TLeft, TNewRight> SelectRightWrapped<TNewRight>(Func<TRight, Either<TLeft, TNewRight>> selector)
        => IsRight ? selector(_right) : new(_left);

    public Either<TNewLeft, TNewRight> Select<TNewLeft, TNewRight>(Func<TLeft, TNewLeft> leftSelector, Func<TRight, TNewRight> rightSelector)
        => IsLeft ? new(leftSelector(_left)) : new(rightSelector(_right));

    public Either<TNewLeft, TNewRight> SelectWrapped<TNewLeft, TNewRight>(Func<TLeft, Either<TNewLeft, TNewRight>> leftSelector, Func<TRight, Either<TNewLeft, TNewRight>> rightSelector)
        => IsLeft ? leftSelector(_left) : rightSelector(_right);

    #endregion
}
