namespace Trarizon.Library.Functional;

public static class FunctorExtensions
{
    #region Convertions

#if OPTIONAL

    public static T? ToNullable<T>(this in Optional<T> optional) where T : struct
        => optional.HasValue ? optional.Value : null;

#endif

#if OPTIONAL && RESULT

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, TError error)
        => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(error);

    public static Result<T, TError> ToResult<T, TError>(this in Optional<T> optional, Func<TError> errorSelector)
        => optional.HasValue ? Result.Success(optional.Value) : Result.Failure(errorSelector());

    public static Optional<T> ToOptional<T, TError>(this in Result<T, TError> result)
        => result.IsSuccess ? Optional.Of(result._value) : Optional.None;

    public static Optional<TError> ToOptionalError<T, TError>(this in Result<T, TError> result)
        => result.IsFailure ? Optional.Of(result.Error) : Optional.None;

    public static Result<Optional<T>, TError> Transpose<T, TError>(this in Optional<Result<T, TError>> optional)
    {
        if (optional.HasValue) {
            ref readonly var result = ref optional.GetValueRefOrDefaultRef();
            return result.IsSuccess ? Optional.Of(result._value) : Result.Failure(result.Error);
        }
        return Result.Success<Optional<T>>(Optional.None);
    }

    public static Optional<Result<T, TError>> Transpose<T, TError>(this in Result<Optional<T>, TError> result)
    {
        if (result.IsSuccess) {
            ref readonly var optional = ref result._value;
            return optional.HasValue ? Optional.Of(Result.Success<T, TError>(optional.Value)) : Optional.None;
        }
        else {
            return Optional.Of(Result.Failure<T, TError>(result.Error));
        }
    }

#endif

    #endregion

    #region Enumerable

#if OPTIONAL

    /// <summary>
    /// Collect all values in Optional&lt;T>, return None if one of item is None
    /// </summary>
    /// <returns></returns>
    public static Optional<IEnumerable<T>> Collect<T>(this IEnumerable<Optional<T>> optionals)
    {
        var values = new List<T>();
        foreach (var optional in optionals) {
            if (optional.HasValue)
                values.Add(optional.Value);
            else
                return Optional.None;
        }
        return values;
    }
    
    /// <summary>
    /// Filters an sequence of optional values and returns a new sequence containing those that has value
    /// </summary>
    public static IEnumerable<T> WhereHasValue<T>(this IEnumerable<Optional<T>> source)
        => source.Where(x => x.HasValue).Select(x => x.Value);

#endif

#if RESULT

    /// <summary>
    /// Collect all values in Result&lt;T>, return Failure of first error if one of item is Failure
    /// </summary>
    /// <returns></returns>
    public static Result<IEnumerable<T>, TError> Collect<T, TError>(this IEnumerable<Result<T, TError>> results)
    {
        var values = new List<T>();
        foreach (var result in results) {
            if (result.IsSuccess)
                values.Add(result._value);
            else
                return Result.Failure(result.Error);
        }
        return values;
    }

    public static IEnumerable<T> WhereIsSuccess<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsSuccess).Select(x => x.Value);

    public static IEnumerable<TError> WhereIsFailure<T, TError>(this IEnumerable<Result<T, TError>> source)
        => source.Where(x => x.IsFailure).Select(x => x.Error!);

#endif

    #endregion
}
