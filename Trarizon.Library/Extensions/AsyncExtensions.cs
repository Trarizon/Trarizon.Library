﻿namespace Trarizon.Library.Extensions;
public static partial class AsyncExtensions
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
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static void Sync(this in ValueTask task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static T Sync<T>(this in ValueTask<T> task) => task.GetAwaiter().GetResult();

    /// <summary>
    /// Run synchronously by call GetAwaiter().GetResult()
    /// </summary>
    public static void Sync<T>(this in ValueTask? task) => task.GetAwaiter().GetResult();

    #endregion

    #region GetAwaiter

    /// <summary>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </summary>
    public static NullableValueTaskAwaiter GetAwaiter(this ValueTask? valueTask) => new(valueTask);

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
