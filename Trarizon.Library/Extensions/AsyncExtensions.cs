namespace Trarizon.Library.Extensions;
public static partial class AsyncExtensions
{
    #region Sync

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static void Sync(this Task task)
    {
        if (!task.IsCompleted)
            task.GetAwaiter().GetResult();
        else if (task.IsCompletedSuccessfully)
            return;
        else // faulted or cancelled
            throw task.Exception!;
    }

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static T Sync<T>(this Task<T> task)
    {
        if (!task.IsCompleted)
            return task.GetAwaiter().GetResult();
        else if (task.IsCompletedSuccessfully)
            return task.Result;
        else // faulted or cancelled
            throw task.Exception!;
    }

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static void Sync(this in ValueTask task)
    {
        if (task.IsCompletedSuccessfully)
            return;
        else
            task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static T Sync<T>(this in ValueTask<T> task)
    {
        if (task.IsCompleted)
            return task.Result;
        else
            return task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static void Sync(this in ValueTask? task)
    {
        if (task.HasValue)
            task.Value.Sync();
    }

    #endregion

    #region GetAwaiter

    /// <summary>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </summary>
    public static NullableValueTaskAwaiter GetAwaiter(this ValueTask? valueTask) => new(valueTask);

    #endregion

    #region Select

    public static ValueTask<TResult> Select<T, TResult>(this ValueTask<T> task, Func<T, TResult> selector)
    {
        return task.IsCompletedSuccessfully
            ? ValueTask.FromResult(selector(task.Result))
            : Otherwise(task, selector);
        static async ValueTask<TResult> Otherwise(ValueTask<T> task, Func<T, TResult> selector) => selector(await task);
    }

    public static Task<TResult> Select<T, TResult>(this Task<T> task, Func<T, TResult> selector)
    {
        return task.IsCompletedSuccessfully
            ? Task.FromResult(selector(task.Result))
            : Otherwise(task, selector);
        static async Task<TResult> Otherwise(Task<T> task, Func<T, TResult> selector) => selector(await task);
    }

    #endregion
}
