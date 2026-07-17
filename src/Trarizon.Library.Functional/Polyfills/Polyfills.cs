namespace Trarizon.Library.Functional;

internal static class Polyfills
{
#if NETSTANDARD2_0

    extension<T>(Task<T> task)
    {
        public bool IsCompletedSuccessfully => task.Status == TaskStatus.RanToCompletion;
    }

#endif

#if EXT_VALUE_TASK && NETSTANDARD2_1

    extension(ValueTask)
    {
        public static ValueTask<T> FromResult<T>(T value) => new(value);
    }

#endif
}
