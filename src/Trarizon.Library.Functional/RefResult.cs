using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

#if RESULT && NET9_0_OR_GREATER

public static class RefResult
{
    extension(Result)
    {
        public static RefResult<T, TError> Success<T, TError>(T value) where T : allows ref struct where TError : allows ref struct
            => new(value);

        public static SuccessBuilder<T> Success<T>(T value) where T : allows ref struct
            => new(value);

        public static RefResult<T, TError> Failure<T, TError>(TError error) where T : allows ref struct where TError : allows ref struct
            => new(error);

        public static FailureBuilder<TError> Failure<TError>(TError error) where TError : allows ref struct
            => new(error);

        public static RefResult<T, TError> Create<T, TError>(bool isSuccess, T value, TError error) where T : allows ref struct where TError : allows ref struct
            => isSuccess ? new(value) : new(error);
    }

    public static ref readonly T? GetValueRefOrDefaultRef<T, TError>(this ref readonly RefResult<T, TError> result)
        where T : allows ref struct where TError : allows ref struct
        => ref result._value;

    public static ref readonly TError? GetErrorRefOrDefaultRef<T, TError>(this ref readonly RefResult<T, TError> result)
        where T : allows ref struct where TError : allows ref struct
        => ref result._error;

    public static Result<T, TError> AsDeref<T, TError>(this RefResult<T, TError> self) => self;
    public static RefResult<T, TError> AsRef<T, TError>(this Result<T, TError> self) => self;

    public static RefResult<TResult, TError> Select<T, TError, TResult>(this Result<T, TError> self, Func<T, TResult> selector)
        where TResult : allows ref struct
        => self.IsSuccess ? new(selector(self._value)) : new(self._error);

    public static SuccessBuilder<TResult> Select<T, TResult>(this Result.SuccessBuilder<T> self, Func<T, TResult> selector)
        where TResult : allows ref struct
        => new(selector(self._value));

    public static RefResult<T, TResult> SelectError<T, TError, TResult>(this Result<T, TError> self, Func<TError, TResult> selector)
        where TResult : allows ref struct
        => self.IsFailure ? new(selector(self._error)) : new(self._value);

    public static FailureBuilder<TResult> SelectError<T, TResult>(this Result.FailureBuilder<T> self, Func<T, TResult> selector)
        where TResult : allows ref struct
        => new(selector(self._error));

    public static RefResult<TResult, TResultError> Select<T, TError, TResult, TResultError>(this RefResult<T, TError> self, Func<T, TResult> valueSelector, Func<TError, TResultError> errorSelector)
        where TResult : allows ref struct where TResultError : allows ref struct
        => self.IsSuccess ? new(valueSelector(self._value)) : new(errorSelector(self._error));

    public static RefResult<TResult, TError> Bind<T, TError, TResult>(this Result<T, TError> self, Func<T, RefResult<TResult, TError>> selector)
        where TResult : allows ref struct
        => self.IsSuccess ? selector(self._value) : new(self._error);

    public static RefResult<TResult, TError> Bind<T, TResult, TError>(this Result.SuccessBuilder<T> self, Func<T, RefResult<TResult, TError>> selector)
        where TResult : allows ref struct where TError : allows ref struct
        => selector(self._value);

    public static RefResult<T, TResultError> BindError<T, TError, TResultError>(this RefResult<T, TError> self, Func<TError, RefResult<T, TResultError>> selector)
        where TResultError : allows ref struct
        => self.IsSuccess ? new(self._value) : selector(self._error);

    public static RefResult<TResult, TResultError> BindError<TError, TResult, TResultError>(this Result.FailureBuilder<TError> self, Func<TError, RefResult<TResult, TResultError>> selector)
        where TResult : allows ref struct where TResultError : allows ref struct
        => selector(self._error);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref struct SuccessBuilder<T> where T : allows ref struct
    {
        internal readonly T _value;
        internal SuccessBuilder(T value) => _value = value;
        public RefResult<T, TError> Build<TError>() where TError : allows ref struct => _value;

        public bool IsSuccess => true;
        public bool IsFailure => false;
        public T Value => _value;
        public FailureBuilder<T> Swap() => new(_value);
        public SuccessBuilder<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : allows ref struct => new(selector(_value));
        public RefResult<TResult, TError> Bind<TResult, TError>(Func<T, RefResult<TResult, TError>> selector) where TResult : allows ref struct where TError : allows ref struct => selector(_value);

#pragma warning disable CS0809
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Not supported for ref struct")]
        public override string ToString() => throw new NotSupportedException();
#pragma warning restore CS0809
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref struct FailureBuilder<TError> where TError : allows ref struct
    {
        internal readonly TError _error;
        internal FailureBuilder(TError error) => _error = error;
        public RefResult<T, TError> Build<T>() where T : allows ref struct => _error;

        public bool IsSuccess => false;
        public bool IsFailure => true;
        public TError Error => _error;
        public SuccessBuilder<TError> Swap() => new(_error);
        public FailureBuilder<TNewError> SelectError<TNewError>(Func<TError, TNewError> selector) where TNewError : allows ref struct => new(selector(_error));
        public RefResult<TResult, TResultError> BindError<TResult, TResultError>(Func<TError, RefResult<TResult, TResultError>> selector) where TResult : allows ref struct where TResultError : allows ref struct => selector(_error);

#pragma warning disable CS0809
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Not supported for ref struct")]
        public override string ToString() => throw new NotSupportedException();
#pragma warning restore CS0809
    }
}

public readonly ref struct RefResult<T, TError>
    where T : allows ref struct where TError : allows ref struct
{
    internal readonly bool _success;
    internal readonly T? _value;
    internal readonly TError? _error;

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess => _success;

    [MemberNotNullWhen(false, nameof(_value))]
    [MemberNotNullWhen(true, nameof(_error))]
    public bool IsFailure => !_success;

    /// <summary>
    /// Unlike <see cref="Nullable{T}.Value"/>, this property won't throw
    /// when optional has no value.
    /// </summary>
    public T Value => _value!;

    public TError Error => _error!;

    public T GetValueOrThrow()
    {
        if (!IsSuccess)
            ResultException.Throw<T, TError>(_error);
        return _value;
    }

    public TError GetErrorOrThrow()
    {
        if (IsSuccess)
            ResultException.Throw<T, TError>(_value);
        return _error;
    }

    public T? GetValueOrDefault() => _value;

    public TError? GetErrorOrDefault() => _error;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValueOrDefault(T? defaultValue)
        => IsSuccess ? _value : defaultValue;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public TError? GetErrorOrDefault(TError? defaultValue)
        => IsFailure ? _error : defaultValue;

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError error)
    {
        value = _value;
        error = _error;
        return IsSuccess;
    }

    [MemberNotNullWhen(true, nameof(_value)), MemberNotNullWhen(false, nameof(_error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsSuccess;
    }

    [MemberNotNullWhen(false, nameof(_value)), MemberNotNullWhen(true, nameof(_error))]
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        error = _error;
        return IsFailure;
    }

    internal RefResult(bool success, T? value, TError? error)
    {
        _success = success;
        _value = value;
        _error = error;
    }

    public RefResult(T value)
    {
        _success = true;
        _value = value;
    }

    public RefResult(TError error)
    {
        _success = false;
        _error = error;
    }

    public static implicit operator RefResult<T, TError>(T value) => new(value);
    public static implicit operator RefResult<T, TError>(TError error) => new(error);
    public static implicit operator RefResult<T, TError>(RefResult.SuccessBuilder<T> builder) => new(builder._value);
    public static implicit operator RefResult<T, TError>(RefResult.FailureBuilder<TError> builder) => new(builder._error);

    public RefResult<TError, T> Swap() => new(!_success, _error, _value);

    public TResult Match<TResult>(Func<T, TResult> valueSelector, Func<TError, TResult> errorSelector)
        where TResult : allows ref struct
        => IsSuccess ? valueSelector(_value) : errorSelector(_error);

    public void Match(Action<T>? valueSelector, Action<TError>? errorSelector)
    {
        if (IsSuccess)
            valueSelector?.Invoke(_value);
        else
            errorSelector?.Invoke(_error);
    }

    public void MatchValue(Action<T> selector)
    {
        if (IsSuccess) selector(_value);
    }

    public void MatchError(Action<TError> selector)
    {
        if (IsFailure) selector(_error);
    }

    public RefResult<TResult, TError> Select<TResult>(Func<T, TResult> valueSelector)
        where TResult : allows ref struct
        => IsSuccess ? new(valueSelector(_value)) : new(_error);

    public RefResult<T, TResult> SelectError<TResult>(Func<TError, TResult> errorSelector)
        where TResult : allows ref struct
        => IsSuccess ? new(_value) : new(errorSelector(_error));

    public RefResult<TResult, TResultError> Select<TResult, TResultError>(Func<T, TResult> valueSelector, Func<TError, TResultError> errorSelector)
        where TResult : allows ref struct where TResultError : allows ref struct
        => IsSuccess ? new(valueSelector(_value)) : new(errorSelector(_error));

    public RefResult<TResult, TError> Bind<TResult>(Func<T, RefResult<TResult, TError>> selector)
        where TResult : allows ref struct
        => IsSuccess ? selector(_value) : new(_error);

    public RefResult<T, TResultError> BindError<TResultError>(Func<TError, RefResult<T, TResultError>> selector)
        where TResultError : allows ref struct
        => IsSuccess ? new(_value) : selector(_error);

#pragma warning disable CS0809

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Not supported for ref struct")]
    public override string ToString() => throw new NotSupportedException();

#pragma warning restore CS0809
}

partial struct Result<T, TError>
{
    public static implicit operator RefResult<T, TError>(Result<T, TError> result) => new(result._success, result._value, result._error);
    public static implicit operator Result<T, TError>(RefResult<T, TError> result) => new(result._success, result._value, result._error);
}

#endif
