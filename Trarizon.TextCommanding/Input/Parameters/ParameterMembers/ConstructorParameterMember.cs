using System.Reflection;
using Trarizon.TextCommanding.Attributes;

namespace Trarizon.TextCommanding.Input.Parameters.ParameterMembers;
internal sealed class ConstructorParameterMember(ParameterInfo info) : IParameterMember
{
    public string? MemberName => info.Name;

    public Type MemberType => info.ParameterType;

    public IEnumerable<TCParameterAttribute> GetParameterAttributes() => info.GetCustomAttributes<TCParameterAttribute>();
}
