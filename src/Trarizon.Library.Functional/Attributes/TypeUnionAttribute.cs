namespace Trarizon.Library.Functional.Attributes;

public enum UnionShareInterfaceOption { Disabled, Enabled, Explicit, }

[AttributeUsage(AttributeTargets.Struct)]
public class TypeUnionAttribute(params Type[] types) : Attribute
{
    public Type[] Types { get; } = types;
    public UnionShareInterfaceOption ShareInterface { get; set; }
}

public sealed class TypeUnionAttribute<T1, T2>() : TypeUnionAttribute(typeof(T1), typeof(T2))
#if NET9_0_OR_GREATER
    where T1 : allows ref struct
    where T2 : allows ref struct
#endif
    ;

