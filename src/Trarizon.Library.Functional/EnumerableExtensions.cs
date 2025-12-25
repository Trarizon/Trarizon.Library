namespace Trarizon.Library.Functional;

#if EXT_ENUMERABLE

public static class EnumerableExtensions
{
    /// <summary>
    /// Collect all values in Optional&lt;T>, return None if one of item is None
    /// </summary>
    /// <returns></returns>
    public static Optional<IEnumerable<T>> Collect<T>(this IEnumerable<Optional<T>> optionals)
    {
        using var enumerator = optionals.GetEnumerator();

        if (!enumerator.MoveNext())
            return Optional.Of(Enumerable.Empty<T>());

        var optional = enumerator.Current;
        if (!optional.HasValue)
            return Optional.None;

        var values = new List<T>() { optional.Value };

        while (enumerator.MoveNext()) {
            optional = enumerator.Current;
            if (!optional.HasValue)
                return Optional.None;
            values.Add(optional.Value);
        }
        return values;
    }

    /// <summary>
    /// Filters an sequence of optional values and returns a new sequence containing those that has value
    /// </summary>
    public static IEnumerable<T> WhereHasValue<T>(this IEnumerable<Optional<T>> source)
        => source.Where(x => x.HasValue).Select(x => x.Value);

    /// <summary>
    /// Collect all values in Result&lt;T>, return Failure of first error if one of item is Failure
    /// </summary>
    /// <returns></returns>
    public static Result<IEnumerable<T>, TError> Collect<T, TError>(this IEnumerable<Result<T, TError>> results)
    {
        using var enumerator = results.GetEnumerator();

        if (!enumerator.MoveNext())
            return Result.Success(Enumerable.Empty<T>());

        var result = enumerator.Current;
        if (result.IsFailure)
            return Result.Failure(result.Error);

        var values = new List<T>() { result.Value };

        while (enumerator.MoveNext()) {
            result = enumerator.Current;
            if (result.IsFailure)
                return Result.Failure(result.Error);
            values.Add(result.Value);
        }
        return values;
    }

    public static IEnumerable<T> WhereIsSuccess<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsSuccess).Select(x => x.Value);

    public static IEnumerable<TError> WhereIsFailure<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsFailure).Select(x => x.Error!);
}

#endif
