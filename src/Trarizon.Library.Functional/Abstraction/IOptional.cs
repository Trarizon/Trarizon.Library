namespace Trarizon.Library.Functional.Abstraction;

internal interface IOptional<out T>
{
    bool HasValue { get; }
    T Value { get; }
}

internal static class OptionalExtensions
{
    public static IOptional<TResult> Select<T, TResult>(IOptional<T> optional, Func<T, TResult> selector)
    {
        if (optional.HasValue)
            return Optional.Of(selector(optional.Value));
        else
            return Optional<TResult>.None;
    }
}
