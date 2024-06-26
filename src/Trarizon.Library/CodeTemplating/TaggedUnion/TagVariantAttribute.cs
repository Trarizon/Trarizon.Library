﻿namespace Trarizon.Library.CodeTemplating.TaggedUnion;
[AttributeUsage(AttributeTargets.Field)]
public class TagVariantAttribute(params Type[] types) : Attribute
{
    public Type[] Types => types;
    public string?[]? Identifiers { get; init; }

    internal TagVariantAttribute(Type[] types, string?[]? identifiers) : this(types)
        => Identifiers = identifiers;
}

public sealed class TagVariantAttribute<T>(string? name = null)
    : TagVariantAttribute([typeof(T)], [name]);

public sealed class TagVariantAttribute<T1, T2>(string? name1, string? name2)
    : TagVariantAttribute([typeof(T1), typeof(T2)], [name1, name2]);

public sealed class TagVariantAttribute<T1, T2, T3>(string? name1, string? name2, string? name3)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3)], [name1, name2, name3]);

public sealed class TagVariantAttribute<T1, T2, T3, T4>(string? name1, string? name2, string? name3, string? name4)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4)], [name1, name2, name3, name4]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5>(string? name1, string? name2, string? name3, string? name4, string? name5)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)], [name1, name2, name3, name4, name5]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5, T6>(string? name1, string? name2, string? name3, string? name4, string? name5, string? name6)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)], [name1, name2, name3, name4, name5, name6]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5, T6, T7>(string? name1, string? name2, string? name3, string? name4, string? name5, string? name6, string? name7)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)], [name1, name2, name3, name4, name5, name6, name7]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5, T6, T7, T8>(string? name1, string? name2, string? name3, string? name4, string? name5, string? name6, string? name7, string? name8)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)], [name1, name2, name3, name4, name5, name6, name7, name8]);
