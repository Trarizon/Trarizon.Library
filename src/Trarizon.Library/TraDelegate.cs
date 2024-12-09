using System.Runtime.CompilerServices;

namespace Trarizon.Library;
public static class TraDelegate
{
    public static unsafe Action Create<TObj>(TObj obj, delegate*<TObj, void> methodPtr) where TObj : class
        => Utils.CreateAction(obj, (nint)methodPtr);

#if NET9_0_OR_GREATER

    public static unsafe Func<T> Create<TObj, T>(TObj obj, delegate*<TObj, T> methodPtr) where TObj : class
        => Utils<T>.CreateFunc(obj, (nint)methodPtr);

    public static unsafe Action<T> Create<TObj, T>(TObj obj, delegate*<TObj, T, void> methodPtr) where TObj : class
        => Utils<T>.CreateAction(obj, (nint)methodPtr);

    public static unsafe Func<T1, TReturn> Create<TObj, T1, TReturn>(TObj obj, delegate*<TObj, T1, TReturn> methodPtr) where TObj : class
        => Utils<T1, TReturn>.CreateFunc(obj, (nint)methodPtr);

    public static unsafe Action<T1, T2> Create<TObj, T1, T2>(TObj obj, delegate*<TObj, T1, T2, void> methodPtr) where TObj : class
        => Utils<T1, T2>.CreateAction(obj, (nint)methodPtr);

    public static unsafe Func<T1, T2, TReturn> Create<TObj, T1, T2, TReturn>(TObj obj, delegate*<TObj, T1, T2, TReturn> methodPtr) where TObj : class
        => Utils<T1, T2, TReturn>.CreateFunc(obj, (nint)methodPtr);

    public static unsafe Action<T1, T2, T3> Create<TObj, T1, T2, T3>(TObj obj, delegate*<TObj, T1, T2, T3, void> methodPtr) where TObj : class
        => Utils<T1, T2, T3>.CreateAction(obj, (nint)methodPtr);

    public static unsafe Func<T1, T2, T3, TReturn> Create<TObj, T1, T2, T3, TReturn>(TObj obj, delegate*<TObj, T1, T2, T3, TReturn> methodPtr) where TObj : class
        => Utils<T1, T2, T3, TReturn>.CreateFunc(obj, (nint)methodPtr);

    public static unsafe Action<T1, T2, T3, T4> Create<TObj, T1, T2, T3, T4>(TObj obj, delegate*<TObj, T1, T2, T3, T4, void> methodPtr) where TObj : class
        => Utils<T1, T2, T3, T4>.CreateAction(obj, (nint)methodPtr);

    public static unsafe Func<T1, T2, T3, T4, TReturn> Create<TObj, T1, T2, T3, T4, TReturn>(TObj obj, delegate*<TObj, T1, T2, T3, T4, TReturn> methodPtr) where TObj : class
        => Utils<T1, T2, T3, T4, TReturn>.CreateFunc(obj, (nint)methodPtr);

#endif

    private static class Utils
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action CreateAction(object obj, nint methodPtr);
    }

#if NET9_0_OR_GREATER

    private static class Utils<T>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action<T> CreateAction(object obj, nint methodPtr);
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Func<T> CreateFunc(object obj, nint methodPtr);
    }

    private static class Utils<T1, T2>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action<T1, T2> CreateAction(object obj, nint methodPtr);
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Func<T1, T2> CreateFunc(object obj, nint methodPtr);
    }

    private static class Utils<T1, T2, T3>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action<T1, T2, T3> CreateAction(object obj, nint methodPtr);
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Func<T1, T2, T3> CreateFunc(object obj, nint methodPtr);
    }

    private static class Utils<T1, T2, T3, T4>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action<T1, T2, T3, T4> CreateAction(object obj, nint methodPtr);
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Func<T1, T2, T3, T4> CreateFunc(object obj, nint methodPtr);
    }

    private static class Utils<T1, T2, T3, T4, T5>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action<T1, T2, T3, T4, T5> CreateAction(object obj, nint methodPtr);
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Func<T1, T2, T3, T4, T5> CreateFunc(object obj, nint methodPtr);
    }

#endif
}
