using System.Diagnostics;

namespace Trarizon.Library.CodeAnalysis.MemberAccess;
[AttributeUsage(AttributeTargets.Field)]
[Conditional("CODE_ANALYSIS")]
public sealed class BackingFieldAccessAttribute(params string[] accessableMembers) : Attribute
{
    public string[] AccessableMembers => accessableMembers;
}
