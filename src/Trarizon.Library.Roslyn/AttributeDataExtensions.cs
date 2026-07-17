using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Trarizon.Library.Functional;
using AttributeData = Microsoft.CodeAnalysis.AttributeData;
using TypedConstant = Microsoft.CodeAnalysis.TypedConstant;

namespace Trarizon.Library.Roslyn;

public static class AttributeDataExtensions
{
    public static Optional<TypedConstant> GetConstructorArgument(this AttributeData attributeData, int index)
    {
        var args = attributeData.ConstructorArguments;
        if ((uint)index < (uint)args.Length)
            return args[index];
        return default;
    }

    public static Optional<TypedConstant> GetNamedArgument(this AttributeData attributeData, string parameterName)
    {
        foreach (var arg in attributeData.NamedArguments) {
            if (arg.Key == parameterName)
                return arg.Value;
        }
        return default;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? CastValueOrDefault<T>(this in Optional<TypedConstant> typedConstant, T? defaultValue = default)
    {
        if (typedConstant.HasValue)
            return typedConstant.Value.CastValue<T>();
        else
            return defaultValue;
    }

    public static ImmutableArray<T> CastArray<T>(this in Optional<TypedConstant> typedConstant)
    {
        if (typedConstant.HasValue) {
            return typedConstant.Value.CastArray<T>();
        }
        else {
            return [];
        }
    }

    public static T CastValue<T>(this TypedConstant typedConstant)
    {
        return (T)typedConstant.Value!;
    }

    public static ImmutableArray<T> CastArray<T>(this TypedConstant constant)
    {
        var arr = constant.Values;
        if (arr is [])
            return [];

        var values = new T[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            values[i] = arr[i].CastValue<T>();
        }
        return ImmutableCollectionsMarshal.AsImmutableArray(values);
    }
}
