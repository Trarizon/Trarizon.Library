using Trarizon.TextCommanding.Attributes;

namespace Trarizon.TextCommanding.Input.Parameters.ParameterMembers;
internal interface IParameterMember
{
    string? MemberName { get; }
    Type MemberType { get; }

    IEnumerable<TCParameterAttribute> GetParameterAttributes();
}
