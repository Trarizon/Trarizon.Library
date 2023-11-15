using System.Collections.Concurrent;
using System.Reflection;

namespace Trarizon.TextCommanding.Utility;
internal static class ReflectionUtil
{
    private static readonly MethodInfo EmptyArrayMethod = typeof(Array).GetMethod("Empty")!;
    private static readonly Type[] IParsableParametersTypes = new[] { typeof(string), typeof(IFormatProvider) };
    private static readonly Type[] ParseParametersTypes = new[] { typeof(string) };

    private static readonly ConcurrentDictionary<Type, object> EmptyArrays = new();
    private static readonly ConcurrentDictionary<Type, Type> GenericListTypes = new();
    private static readonly ConcurrentDictionary<Type, (MethodInfo, bool)> ParseMethods = new();

    public static object? InvokeStatic(this MethodInfo method, params object?[] parameters)
        => method.Invoke(null, parameters);

    public static object GetEmptyArray(Type elementType) => EmptyArrays.GetOrAdd(elementType, static type =>
    {
        return EmptyArrayMethod.MakeGenericMethod(type).InvokeStatic()!;
    });

    public static Type GetGenericListType(Type elementType) => GenericListTypes.GetOrAdd(elementType, static type =>
    {
        return typeof(List<>).MakeGenericType(type);
    });

    public static bool TryParseText(Type type, string text, out object? result)
    {
        (MethodInfo, bool RequiresFormatProvider) res = ParseMethods.GetOrAdd(type, static type =>
        {
            var method = type.GetMethod("Parse", ParseParametersTypes);
            if (method != null)
                return (method, false);

            method = type.GetMethod("Parse", IParsableParametersTypes);
            if (method != null)
                return (method, true);

            return default;
        });

        if (res == default) {
            result = default;
            return false;
        }

        result = res.RequiresFormatProvider
            ? res.Item1.InvokeStatic(text, default(IFormatProvider))
            : res.Item1.InvokeStatic(text);
        return true;
    }
}
