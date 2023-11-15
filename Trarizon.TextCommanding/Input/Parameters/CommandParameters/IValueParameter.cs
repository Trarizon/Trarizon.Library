using Trarizon.TextCommanding.Attributes;
using Trarizon.TextCommanding.Input.Parameters.ParameterMembers;

namespace Trarizon.TextCommanding.Input.Parameters.CommandParameters;
internal interface IValueParameter<out TAttribute> where TAttribute : TCValueAttribute
{
    public TAttribute Attribute { get; }
    public IParameterMember ParameterMember { get; }

    object? ParseInput(scoped ref RawArguments.Values input);
}
