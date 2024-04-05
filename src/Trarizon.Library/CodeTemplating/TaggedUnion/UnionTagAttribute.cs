namespace Trarizon.Library.CodeTemplating.TaggedUnion;
[AttributeUsage(AttributeTargets.Enum)]
public sealed class UnionTagAttribute(string? generatedTypeName = null) : Attribute
{
    public string? GeneratedTypeName => generatedTypeName;
}
