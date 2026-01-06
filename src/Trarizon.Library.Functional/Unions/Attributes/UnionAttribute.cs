namespace Trarizon.Library.Functional.Unions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UnionAttribute(params Type[] types) : Attribute
{
    public Type[] Types { get; } = types;
}
