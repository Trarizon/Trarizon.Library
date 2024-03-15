using System.Diagnostics;

namespace Trarizon.Library.CodeAnalysis.MemberAccess;
[AttributeUsage(AttributeTargets.Field)]
[Conditional("CODE_ANALYSIS")]
public sealed class BackingFieldAccessAttribute(params string[] accessabkeMembers) : Attribute
{
    public string[] AccessableMembers => accessabkeMembers;
}
