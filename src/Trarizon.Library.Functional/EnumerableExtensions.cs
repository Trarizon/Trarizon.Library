namespace Trarizon.Library.Functional;

public static class EnumerableExtensions
{
    /// <summary>
    /// Filters an sequence of optional values and returns a new sequence containing those that has value
    /// </summary>
    public static IEnumerable<T> WhereHasValue<T>(this IEnumerable<Optional<T>> source)
        => source.Where(x => x.HasValue).Select(x => x.Value);

    public static IEnumerable<T> WhereIsSuccess<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsSuccess).Select(x => x.Value);

    public static IEnumerable<TError> WhereIsFailure<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsFailure).Select(x => x.Error!);
}
