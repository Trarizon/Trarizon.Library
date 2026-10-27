namespace Trarizon.Library.Linq;

interface ITransformer<T, U>
{
    U Transform(T value);
}

internal readonly struct SelfTf<T> : ITransformer<T, T>
{
    public T Transform(T value) => value;
}

internal readonly struct FuncTf<T, U>(Func<T, U> transform) : ITransformer<T, U>
{
    public U Transform(T value) => transform(value);
}