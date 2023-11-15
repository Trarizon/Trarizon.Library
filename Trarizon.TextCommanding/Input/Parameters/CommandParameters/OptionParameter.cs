using Trarizon.TextCommanding.Attributes;
using Trarizon.TextCommanding.Input.Parameters.ParameterMembers;

namespace Trarizon.TextCommanding.Input.Parameters.CommandParameters;
internal sealed class OptionParameter(IParameterMember parameterMember, TCOptionAttribute attribute)
{
    public TCOptionAttribute Attribute => attribute;

    public IParameterMember ParameterMember => parameterMember;

    public object? ParseValue(string rawInput)
        => RawInputParser.Parse(parameterMember.MemberType, rawInput);
}
