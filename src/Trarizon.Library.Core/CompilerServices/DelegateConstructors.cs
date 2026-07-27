using System.Runtime.CompilerServices;

namespace Trarizon.Library.CompilerServices;

#if NET8_0_OR_GREATER

public static partial class DelegateConstructors
{
    extension(Action)
    {
        public static unsafe Action Create<TObj>(TObj obj, delegate*<TObj, void> methodPtr) where TObj : class
            => Utils.CreateAction(obj, (nint)methodPtr);
    }

    private static class Utils
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] public static extern Action CreateAction(object obj, nint methodPtr);
    }

#if NET9_0_OR_GREATER

    extension<T>(Action<T>)
    {
        public static unsafe Action<T> Create<TObj>(TObj obj, delegate*<TObj, T, void> methodPtr) where TObj : class
            => Utils<T>.CreateAction(obj, (nint)methodPtr);
    }

    extension<T1, T2>(Action<T1, T2>)
    {
        public static unsafe Action<T1, T2> Create<TObj>(TObj obj, delegate*<TObj, T1, T2, void> methodPtr) where TObj : class
            => Utils<T1, T2>.CreateAction(obj, (nint)methodPtr);
    }

    extension<T>(Func<T>)
    {
        public static unsafe Func<T> Create<TObj>(TObj obj, delegate*<TObj, T> methodPtr) where TObj : class
            => Utils<T>.CreateFunc(obj, (nint)methodPtr);
    }

    extension<T1, T2>(Func<T1, T2>)
    {
        public static unsafe Func<T1, T2> Create<TObj>(TObj obj, delegate*<TObj, T1, T2> methodPtr) where TObj : class
            => Utils<T1, T2>.CreateFunc(obj, (nint)methodPtr);
    }

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

#endif
}

#endif
