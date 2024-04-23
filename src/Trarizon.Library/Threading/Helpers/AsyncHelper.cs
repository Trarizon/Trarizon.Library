using System.Runtime.CompilerServices;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Threading.Helpers;
public static partial class AsyncHelper
{
    #region Sync

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static void Sync(this Task task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static T Sync<T>(this Task<T> task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// Run synchronously
    /// </summary>
    public static void Sync(this in ValueTask task)
    {
        if (task.IsCompleted)
            return;

        task.AsTask().Sync();
    }

    /// <summary>
    /// Run synchronously
    /// </summary>
    public static T Sync<T>(this in ValueTask<T> task)
    {
        if (task.IsCompleted)
            return task.Result;

        return task.AsTask().Sync();
    }

    /// <summary>
    /// Run synchronously
    /// </summary>
    public static void Sync<T>(this in ValueTask? task)
    {
        if (task is null or { IsCompleted: true })
            return;

        Nullable.GetValueRefOrDefaultRef(in task).Sync();
    }

    /// <summary>
    /// Run synchronously
    /// </summary>
    public static Optional<T> Sync<T>(this in ValueTask<T>? task)
    {
        if (task is null)
            return Optional.None<T>();
        return Nullable.GetValueRefOrDefaultRef(in task).Sync();
    }

    #endregion

    #region GetAwaiter

    /// <summary>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </summary>
    public static ValueTaskAwaiter GetAwaiter(this ValueTask? valueTask) => (valueTask ?? ValueTask.CompletedTask).GetAwaiter();

    /// <summary>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </summary>
    public static NullableValueTaskAwaiter<T> GetAwaiter<T>(this ValueTask<T>? valueTask) => new(valueTask);

    #endregion

    #region Select

    public static ValueTask<TResult> Select<T, TResult>(this ValueTask<T> task, Func<T, TResult> selector)
    {
        return task.IsCompletedSuccessfully
            ? ValueTask.FromResult(selector(task.Result))
            : Otherwise(task, selector);
        static async ValueTask<TResult> Otherwise(ValueTask<T> task, Func<T, TResult> selector) => selector(await task.ConfigureAwait(false));
    }

    public static Task<TResult> Select<T, TResult>(this Task<T> task, Func<T, TResult> selector)
    {
        return task.IsCompletedSuccessfully
            ? Task.FromResult(selector(task.Result))
            : Otherwise(task, selector);
        static async Task<TResult> Otherwise(Task<T> task, Func<T, TResult> selector) => selector(await task.ConfigureAwait(false));
    }

    #endregion
}
