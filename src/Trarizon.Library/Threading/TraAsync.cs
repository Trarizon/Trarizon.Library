using System.Runtime.CompilerServices;

namespace Trarizon.Library.Threading;
public static partial class TraAsync
{
    /// <remarks>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </remarks>
    public static ValueTaskAwaiter GetAwaiter(this ValueTask? task) => (task ?? ValueTask.CompletedTask).GetAwaiter();

    /// <remarks>
    /// Provide <see langword="await"/> feature support for nullable <see cref="ValueTask"/>
    /// </remarks>
    public static NullableValueTaskAwaiter<T> GetAwaiter<T>(this ValueTask<T>? task) => new(task);
}
