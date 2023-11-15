using System.Reflection;
using Trarizon.TextCommanding.Attributes;

namespace Trarizon.TextCommanding.Input.Parameters.ParameterMembers;
internal sealed class FieldParameterMember(FieldInfo info) : IParameterProperty
{
    public string? MemberName => info.Name;

    public Type MemberType => info.FieldType;

    public void SetValue(object obj, object? value) => info.SetValue(obj, value);
    public IEnumerable<TCParameterAttribute> GetParameterAttributes() => info.GetCustomAttributes<TCParameterAttribute>();
}
