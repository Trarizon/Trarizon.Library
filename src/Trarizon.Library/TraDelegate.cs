#if !NETSTANDARD

using System.Runtime.CompilerServices;

namespace Trarizon.Library;
public static partial class TraDelegate
{
    public static unsafe Action Create<TObj>(TObj obj, delegate*<TObj, void> methodPtr) where TObj : class
        => Utils.CreateAction(obj, (nint)methodPtr);

    private static class Utils
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action CreateAction(object obj, nint methodPtr);
    }
}

#endif
