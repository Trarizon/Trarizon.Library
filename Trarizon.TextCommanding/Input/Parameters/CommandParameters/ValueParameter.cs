using Trarizon.TextCommanding.Attributes;
using Trarizon.TextCommanding.Input.Parameters.ParameterMembers;

namespace Trarizon.TextCommanding.Input.Parameters.CommandParameters;
internal class ValueParameter(IParameterMember parameterMember, TCValueAttribute attribute) : IValueParameter<TCValueAttribute>
{
    public TCValueAttribute Attribute => attribute;

    public IParameterMember ParameterMember => parameterMember;

    public object? ParseInput(scoped ref RawArguments.Values input)
    {
        if (input.Read(out var value))
            return RawInputParser.Parse(parameterMember.MemberType, value);
        return null;
    }
}
