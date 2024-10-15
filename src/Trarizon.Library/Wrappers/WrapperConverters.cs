using System.Runtime.CompilerServices;
using Trarizon.Library.Threading;

namespace Trarizon.Library.Wrappers;
/// <summary>
/// Covert other wrappers into Optional, etc.
/// </summary>
public static class WrapperConverters
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> ToOptional<T>(this TraAsync.CancellationTaskResult<T> result)
        => new(result._hasValue, result.Result);
}
