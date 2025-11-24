#if EITHER

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

public static class Either
{
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft leftValue)
        => new(leftValue);

    public static LeftBuilder<TLeft> Left<TLeft>(TLeft leftValue)
        => new(leftValue);

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight rightValue)
        => new(rightValue);

    public static RightBuilder<TRight> Right<TRight>(TRight rightValue)
        => new(rightValue);

    public static Either<TLeft, TRight> Create<TLeft, TRight>(bool isLeft, TLeft left, TRight right)
        => isLeft ? new(left) : new(right);

    public static ref readonly TLeft? GetLeftRefOrDefaultRef<TLeft, TRight>(this ref readonly Either<TLeft, TRight> either)
        => ref either._left;

    public static ref readonly TRight? GetRightRefOrDefaultRef<TLeft, TRight>(this ref readonly Either<TLeft, TRight> either)
        => ref either._right;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct LeftBuilder<T>
    {
        internal readonly T _value;
        internal LeftBuilder(T value) => _value = value;
        public Either<T, TRight> Build<TRight>() => _value;

        public bool IsLeft => true;
        public bool IsRight => false;
        public T LeftValue => _value;
        public RightBuilder<T> Swap() => new(_value);
        public LeftBuilder<TResult> CastLeft<TResult>() => new((TResult)(object)_value!);
        public LeftBuilder<TResult> SelectLeft<TResult>(Func<T, TResult> selector) => new(selector(_value));
        public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
        public override string ToString() => Build<object>().ToString();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct RightBuilder<T>
    {
        internal readonly T _value;
        internal RightBuilder(T value) => _value = value;
        public Either<TLeft, T> Build<TLeft>() => _value;

        public bool IsLeft => false;
        public bool IsRight => true;
        public T RightValue => _value;
        public LeftBuilder<T> Swap() => new(_value);
        public RightBuilder<TResult> CastRight<TResult>() => new((TResult)(object)_value!);
        public LeftBuilder<TResult> SelectRight<TResult>(Func<T, TResult> selector) => new(selector(_value));
        public string ToString(bool includeVariantInfo) => Build<object>().ToString(includeVariantInfo);
        public override string ToString() => Build<object>().ToString();
    }
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
            EitherNoValueException.Throw<TLeft, TRight>(left: true);
        return _left;
    }
    public TRight GetValidRightValue()
    {
        if (IsLeft)
            EitherNoValueException.Throw<TLeft, TRight>(left: false);
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
    public static implicit operator Either<TLeft, TRight>(Either.LeftBuilder<TLeft> left) => new(left._value);
    public static implicit operator Either<TLeft, TRight>(Either.RightBuilder<TRight> right) => new(right._value);

    #endregion

    #region Converter

    public Either<TRight, TLeft> Swap()
        => new(!_isLeft, _right, _left);

    public Either<TNewLeft, TRight> CastLeft<TNewLeft>() => IsLeft ? new((TNewLeft)(object)_left) : new(_right);

    public Either<TLeft, TNewRight> CastRight<TNewRight>() => IsRight ? new((TNewRight)(object)_right) : new(_left);

    public Either<TNewLeft, TNewRight> Cast<TNewLeft, TNewRight>() => IsLeft ? new((TNewLeft)(object)_left) : new((TNewRight)(object)_right);

    public TResult Match<TResult>(Func<TLeft, TResult> leftSelector, Func<TRight, TResult> rightSelector)
        => IsLeft ? leftSelector(_left) : rightSelector(_right);

    public void Match(Action<TLeft>? leftSelector, Action<TRight>? rightSelector)
    {
        if (IsLeft)
            leftSelector?.Invoke(_left);
        else
            rightSelector?.Invoke(_right);
    }

    public void MatchLeft(Action<TLeft> leftSelector)
    {
        if (IsLeft) leftSelector(_left);
    }

    public void MatchRight(Action<TRight> rightSelector)
    {
        if (IsRight) rightSelector(_right);
    }

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

    #endregion

    public override string ToString() => (IsLeft ? _left.ToString() : _right.ToString()) ?? string.Empty;

    public string ToString(bool includeVariantInfo)
    {
        if (!includeVariantInfo) {
            return ToString();
        }

        if (IsLeft) {
            string? str;
            if (_left is IMonad monad)
                str = monad.ToString(true);
            else
                str = _left.ToString();
            return str is null ? "Either Left" : $"Left({str})";
        }
        else {
            string? str;
            if (_right is IMonad monad)
                str = monad.ToString(true);
            else
                str = _right.ToString();
            return str is null ? "Either Right" : $"Right({str})";
        }
    }
}

public sealed class EitherNoValueException : InvalidOperationException
{
    private EitherNoValueException(Type leftType, Type rightType, bool left)
        : base($"Either<{leftType.Name}, {rightType.Name}> has no {(left ? "left" : "right")} value")
    { }

    [DoesNotReturn]
    public static void Throw<TLeft, TRight>(bool left) => throw new EitherNoValueException(typeof(TLeft), typeof(TRight), left);
}

#endif
