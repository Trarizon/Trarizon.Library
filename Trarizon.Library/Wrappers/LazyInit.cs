namespace Trarizon.Library.Wrappers;

public struct LazyInit<T>(Func<T> creator)
{
    private Func<T>? _creator = creator;
    private T? _value;

    public readonly bool HasValue => _creator == null;

    public T Value
    {
        get {
            if (!HasValue) {
                _value = _creator!();
                _creator = null;
            }
            return _value!;
        }
    }

    public static implicit operator LazyInit<T>(Func<T> creator) => new(creator);
    public static implicit operator T(LazyInit<T> lazyInitValue) => lazyInitValue.Value;
}

public struct LazyInit<TResult, TArgs>(TArgs args, Func<TArgs, TResult> creator)
{
    private Func<TArgs, TResult>? _creator = creator;
    private TResult? _value;

    public readonly bool HasValue => _creator == null;

    public TResult Value
    {
        get {
            if (!HasValue) {
                _value = _creator!(args);
                _creator = null;
            }
            return _value!;
        }
    }
}