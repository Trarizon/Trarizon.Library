using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static class AttributeDataExtensions
{
    public static Optional<TypedConstant> GetConstructorArgument(this AttributeData attributeData, int index)
    {
        if (attributeData.ConstructorArguments.TryAt(index, out var constant))
            return constant;
        return default;
    }

    public static Optional<TypedConstant> GetNamedArgument(this AttributeData attributeData, string parameterName)
    {
        if (attributeData.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first))
            return first.Value;
        return default;
    }

    public static T Cast<T>(this TypedConstant typedConstant)
    {
        return (T)typedConstant.Value!;
    }

    public static ImmutableArray<T> CastArray<T>(this TypedConstant constant)
    {
        var arr = constant.Values;
        if (arr is [])
            return [];
        return [.. arr.Select(Cast<T>)];
    }
}
