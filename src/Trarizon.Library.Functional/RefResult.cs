using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.Functional;

#if NET9_0_OR_GREATER

public static partial class RefResult
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

        public static RefResult<T, Exception> TryCatch<T>(Func<T> func) where T : allows ref struct
        {
            try { return func(); }
            catch (Exception ex) { return ex; }
        }
    }

    public static ref readonly T? GetValueRefOrDefaultRef<T, TError>(this ref readonly RefResult<T, TError> result)
        where T : allows ref struct where TError : allows ref struct
        => ref result._value;

    public static ref readonly TError? GetErrorRefOrDefaultRef<T, TError>(this ref readonly RefResult<T, TError> result)
        where T : allows ref struct where TError : allows ref struct
        => ref result._error;

    public static Result<T, TError> AsDeref<T, TError>(this RefResult<T, TError> self) => self;
    public static RefResult<T, TError> AsRef<T, TError>(this Result<T, TError> self) => self;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref struct SuccessBuilder<T> where T : allows ref struct
    {
        internal readonly T _value;
        internal SuccessBuilder(T value) => _value = value;
        public RefResult<T, TError> Build<TError>() where TError : allows ref struct => _value;

        public bool IsSuccess => true;
        public bool IsFailure => false;
        public T Value => _value;

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

#pragma warning disable CS0809
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Not supported for ref struct")]
        public override string ToString() => throw new NotSupportedException();
#pragma warning restore CS0809
    }
}

partial class Result
{
    partial struct SuccessBuilder<T>
    {
        public static implicit operator RefResult.SuccessBuilder<T>(SuccessBuilder<T> builder) => new(builder._value);
        public static implicit operator SuccessBuilder<T>(RefResult.SuccessBuilder<T> builder) => new(builder._value);
    }

    partial struct FailureBuilder<TError>
    {
        public static implicit operator RefResult.FailureBuilder<TError>(FailureBuilder<TError> builder) => new(builder._error);
        public static implicit operator FailureBuilder<TError>(RefResult.FailureBuilder<TError> builder) => new(builder._error);
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
