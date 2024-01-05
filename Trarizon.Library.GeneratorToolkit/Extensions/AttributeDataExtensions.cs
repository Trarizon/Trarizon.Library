﻿using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections.Extensions;

namespace Trarizon.Library.GeneratorToolkit.Extensions;
public static class AttributeDataExtensions
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetNamedArgument<T>(this AttributeData attribute, string parameterName, T? defaultValue = default)
        => attribute.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first)
        ? (T)first.Value.Value!
        : defaultValue;

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetConstructorArgument<T>(this AttributeData attribute, int index, T? defaultValue = default)
        => attribute.ConstructorArguments is var args && index >= 0 && index < args.Length
        ? (T)args[index].Value!
        : defaultValue;
}
