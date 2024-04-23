namespace Trarizon.Library.CodeTemplating.TaggedUnion;
[AttributeUsage(AttributeTargets.Field)]
public class TagVariantAttribute(params Type[] types) : Attribute
{
    public Type[] Types => types;
    public string?[]? Identifiers { get; init; }

    internal TagVariantAttribute(Type[] types, string?[]? identifiers) : this(types)
        => Identifiers = identifiers;

    public TagVariantAttribute(Type type1, string? identifier)
        : this([type1], [identifier])
    { }

    public TagVariantAttribute(Type type1, string? identifier1, Type type2, string? identifier2)
        : this([type1, type2], [identifier1, identifier2])
    { }

    public TagVariantAttribute(Type type1, string? identifier1, Type type2, string? identifier2, Type type3, string? identifier3)
        : this([type1, type2, type3], [identifier1, identifier2, identifier3])
    { }

    public TagVariantAttribute(Type type1, string? identifier1, Type type2, string? identifier2, Type type3, string? identifier3, Type type4, string? identifier4)
        : this([type1, type2, type3, type4], [identifier1, identifier2, identifier3, identifier4])
    { }

    public TagVariantAttribute(Type type1, string? identifier1, Type type2, string? identifier2, Type type3, string? identifier3, Type type4, string? identifier4, Type type5, string? identifier5)
        : this([type1, type2, type3, type4, type5], [identifier1, identifier2, identifier3, identifier4, identifier5])
    { }

    public TagVariantAttribute(Type type1, string? identifier1, Type type2, string? identifier2, Type type3, string? identifier3, Type type4, string? identifier4, Type type5, string? identifier5, Type type6, string? identifier6)
        : this([type1, type2, type3, type4, type5, type6], [identifier1, identifier2, identifier3, identifier4, identifier5, identifier6])
    { }

    public TagVariantAttribute(Type type1, string? identifier1, Type type2, string? identifier2, Type type3, string? identifier3, Type type4, string? identifier4, Type type5, string? identifier5, Type type6, string? identifier6, Type type7, string? identifier7)
        : this([type1, type2, type3, type4, type5, type6, type7], [identifier1, identifier2, identifier3, identifier4, identifier5, identifier6, identifier7])
    { }

    public TagVariantAttribute(Type type1, string? identifier1, Type type2, string? identifier2, Type type3, string? identifier3, Type type4, string? identifier4, Type type5, string? identifier5, Type type6, string? identifier6, Type type7, string? identifier7, Type type8, string? identifier8)
        : this([type1, type2, type3, type4, type5, type6, type7, type8], [identifier1, identifier2, identifier3, identifier4, identifier5, identifier6, identifier7, identifier8])
    { }
}

public sealed class TagVariantAttribute<T>(string? name = null)
    : TagVariantAttribute([typeof(T)], [name]);

public sealed class TagVariantAttribute<T1, T2>(string? name1 = null, string? name2 = null)
    : TagVariantAttribute([typeof(T1), typeof(T2)], [name1, name2]);

public sealed class TagVariantAttribute<T1, T2, T3>(string? name1 = null, string? name2 = null, string? name3 = null)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3)], [name1, name2, name3]);

public sealed class TagVariantAttribute<T1, T2, T3, T4>(string? name1 = null, string? name2 = null, string? name3 = null, string? name4 = null)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4)], [name1, name2, name3, name4]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5>(string? name1 = null, string? name2 = null, string? name3 = null, string? name4 = null, string? name5 = null)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)], [name1, name2, name3, name4, name5]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5, T6>(string? name1 = null, string? name2 = null, string? name3 = null, string? name4 = null, string? name5 = null, string? name6 = null)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)], [name1, name2, name3, name4, name5, name6]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5, T6, T7>(string? name1 = null, string? name2 = null, string? name3 = null, string? name4 = null, string? name5 = null, string? name6 = null, string? name7 = null)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)], [name1, name2, name3, name4, name5, name6, name7]);

public sealed class TagVariantAttribute<T1, T2, T3, T4, T5, T6, T7, T8>(string? name1 = null, string? name2 = null, string? name3 = null, string? name4 = null, string? name5 = null, string? name6 = null, string? name7 = null, string? name8 = null)
    : TagVariantAttribute([typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)], [name1, name2, name3, name4, name5, name6, name7, name8]);
