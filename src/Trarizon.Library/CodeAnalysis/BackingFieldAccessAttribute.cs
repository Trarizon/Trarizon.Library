using System.Diagnostics;

namespace Trarizon.Library.CodeAnalysis;
[AttributeUsage(AttributeTargets.Field)]
[Conditional("CODE_ANALYSIS")]
public sealed class BackingFieldAccessAttribute(params string[] accessableMembers) : Attribute
{
    public string[] AccessableMembers => accessableMembers;
}
