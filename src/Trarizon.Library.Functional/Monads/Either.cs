using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Functional.Monads;
public static class Either
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft leftValue)
        => new(leftValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight rightValue)
        => new(rightValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly TLeft? GetLeftRefOrDefaultRef<TLeft, TRight>(this ref readonly Either<TLeft, TRight> either)
        => ref either._left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly TRight? GetRightRefOrDefaultRef<TLeft, TRight>(this ref readonly Either<TLeft, TRight> either)
        => ref either._right;

    #region Conversion

    #region Optional

    public static Optional<TLeft> ToOptionalLeft<TLeft, TRight>(this in Either<TLeft, TRight> either)
        => either.IsLeft ? Optional.Of(either._left) : default;

    public static Optional<TRight> ToOptionalRight<TLeft, TRight>(this in Either<TLeft, TRight> either)
        => either.IsRight ? Optional.Of(either._right) : default;

    #endregion

    #region Result

    public static Result<TLeft, TError> ToResultLeft<TLeft, TRight, TError>(this in Either<TLeft, TRight> either, TError error) where TError : class
        => either.IsLeft ? new(either._left) : new(error);

    public static Result<TLeft, TError> ToResultLeft<TLeft, TRight, TError>(this in Either<TLeft, TRight> either, Func<TRight, TError> errorSelector) where TError : class
        => either.IsLeft ? new(either._left) : new(errorSelector(either._right));

    public static Result<TRight, TError> ToResultRight<TLeft, TRight, TError>(this in Either<TLeft, TRight> either, TError error) where TError : class
        => either.IsRight ? new(either._right) : new(error);

    public static Result<TRight, TError> ToResultRight<TLeft, TRight, TError>(this in Either<TLeft, TRight> either, Func<TLeft, TError> errorSelector) where TError : class
        => either.IsRight ? new(either._right) : new(errorSelector(either._left));

    public static Result<TLeft, TRight> AsResultLeft<TLeft, TRight>(this in Either<TLeft, TRight> either) where TRight : class
        => either.IsLeft ? new(either._left) : new(either._right);

    public static Result<TRight, TLeft> AsResultRight<TLeft, TRight>(this in Either<TLeft, TRight> either) where TLeft : class
        => either.IsRight ? new(either._right) : new(either._left);

    #endregion

    #endregion

    [DoesNotReturn]
    internal static void EitherHasNoValue(bool left) => throw new InvalidOperationException($"Either<,> has no {(left ? "left" : "right")} value");
}

public readonly struct Either<TLeft, TRight>
{
    private readonly bool _isLeft;
    internal readonly TLeft? _left;
    internal readonly TRight? _right;

    #region Accessor

    [MemberNotNullWhen(true, nameof(_left))]
    [MemberNotNullWhen(false, nameof(_right))]
    public bool IsLeft => _isLeft;

    [MemberNotNullWhen(false, nameof(_left))]
    [MemberNotNullWhen(true, nameof(_right))]
    public bool IsRight => !_isLeft;

    public TLeft LeftValue => _left!;
    public TRight RightValue => _right!;

    public TLeft GetValidLeftValue()
    {
        if (IsRight)
            Either.EitherHasNoValue(left: true);
        return _left;
    }
    public TRight GetValidRightValue()
    {
        if (IsLeft)
            Either.EitherHasNoValue(left: true);
        return _right;
    }

    public TLeft? GetLeftValueOrDefault() => _left;
    public TRight? GetRightValueOrDefault() => _right;

    [MemberNotNullWhen(true, nameof(_left)), MemberNotNullWhen(false, nameof(_right))]
    public bool TryGetLeftValue([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
    {
        left = _left;
        right = _right;
        return IsLeft;
    }

    [MemberNotNullWhen(true, nameof(_left)), MemberNotNullWhen(false, nameof(_right))]
    public bool TryGetLeftValue([MaybeNullWhen(false)] out TLeft left)
    {
        left = _left;
        return IsLeft;
    }

    [MemberNotNullWhen(false, nameof(_left)), MemberNotNullWhen(true, nameof(_right))]
    public bool TryGetRightValue([MaybeNullWhen(false)] out TRight right, [MaybeNullWhen(true)] out TLeft left)
        => !TryGetLeftValue(out left, out right);

    [MemberNotNullWhen(false, nameof(_left)), MemberNotNullWhen(true, nameof(_right))]
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

    public Either<TLeft, TNewRight> SelectRight<TNewRight>(Func<TRight, TNewRight> selector)
        => IsRight ? new(selector(_right)) : new(_left);

    public Either<TNewLeft, TNewRight> Select<TNewLeft, TNewRight>(Func<TLeft, TNewLeft> leftSelector, Func<TRight, TNewRight> rightSelector)
        => IsLeft ? new(leftSelector(_left)) : new(rightSelector(_right));

    public Either<TNewLeft, TRight> BindLeft<TNewLeft>(Func<TLeft, Either<TNewLeft, TRight>> selector)
        => IsLeft ? selector(_left) : new(_right);

    public Either<TLeft, TNewRight> BindRight<TNewRight>(Func<TRight, Either<TLeft, TNewRight>> selector)
        => IsRight ? selector(_right) : new(_left);

    public Either<TNewLeft, TNewRight> Bind<TNewLeft, TNewRight>(Func<TLeft, Either<TNewLeft, TNewRight>> leftSelector, Func<TRight, Either<TNewLeft, TNewRight>> rightSelector)
        => IsLeft ? leftSelector(_left) : rightSelector(_right);

    #endregion

    public override string ToString() => (IsLeft ? _left.ToString() : _right.ToString()) ?? string.Empty;
}
