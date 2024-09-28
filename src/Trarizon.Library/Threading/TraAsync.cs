using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
