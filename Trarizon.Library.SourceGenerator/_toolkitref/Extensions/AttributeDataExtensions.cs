using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class AttributeDataExtensions
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetNamedArgument<T>(this AttributeData attribute, string parameterName, T? defaultValue = default)
        => attribute.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first)
        ? (T?)first.Value.Value
        : defaultValue;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetConstructorArgument<T>(this AttributeData attribute, int index, T? defaultValue = default)
        => attribute.ConstructorArguments is var args && index >= 0 && index < args.Length
        ? (T?)args[index].Value
        : defaultValue;

    public static T[]? GetNamedArguments<T>(this AttributeData attribute, string parameterName)
    {
        if (attribute.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first)) {
            var arr = first.Value.Values;
            if (arr.Length == 0)
                return [];
            var result = new T[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                result[i] = (T)arr[i].Value!;
            return result;
        }
        return default;
    }

    public static T[]? GetConstructorArguments<T>(this AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments is var args && index >= 0 && index < args.Length) {
            var arr = args[index].Values;
            if (arr.Length == 0)
                return [];
            var result = new T[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                result[i] = (T)arr[i].Value!;
            return result;
        }
        return default;
    }
}
