namespace Trarizon.Library;
public static class TraEnum
{
    public static bool HasAnyFlag<T>(this T value, T flags) where T : struct, Enum
        => throw new InvalidOperationException("Intercepted by EnumHasAnyFlagInterceptor.");
}
