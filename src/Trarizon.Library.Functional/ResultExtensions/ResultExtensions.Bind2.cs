using System.ComponentModel;

namespace Trarizon.Library.Functional;

public static partial class ResultExtensions
{
    // No 2-parameter Bind overload, we provide the SelectMany to make linq expression available, but manually call Bind wont need it.
    // Developers can use .Select(x => (x, y)).Bind(tpl => Do(tpl.x, tpl.y)) as workaround

    #region SelectMany

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<TResult, TError> SelectMany<T, TError, TMid, TResult>(this Result<T, TError> self, Func<T, Result<TMid, TError>> selector, Func<T, TMid, TResult> resultSelector)
        => self.IsSuccess && selector(self.Value) is { IsSuccess: true } mid ? Result.Success(resultSelector(self.Value, mid.Value)) : Result.Failure(self.Error);

#if REF_MONAD

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<TResult, TError> SelectMany<T, TError, TMid, TResult>(this RefResult<T, TError> self, Func<T, RefResult<TMid, TError>> selector, Func<T, TMid, TResult> resultSelector)
        => self.IsSuccess && selector(self.Value) is { IsSuccess: true } mid ? Result.Success(resultSelector(self.Value, mid.Value)) : Result.Failure(self.Error);

#endif

    #endregion

}
