using Trarizon.TextCommanding.Attributes;
using Trarizon.TextCommanding.Input;
using Trarizon.TextCommanding.Input.Parameters.ParameterMembers;

namespace Trarizon.TextCommanding.Input.Parameters.CommandParameters;
internal sealed class ValuesParameter(IParameterMember parameterMember, TCValuesAttribute attribute) : IValueParameter<TCValuesAttribute>
{
    public TCValuesAttribute Attribute => attribute;

    public IParameterMember ParameterMember => parameterMember;

    public object? ParseInput(scoped ref RawArguments.Values input)
    {
        Type type = parameterMember.MemberType;

        // Array
        if (type.IsArray)
        {
            return RawInputParser.ParseArray(type.GetElementType()!,
                attribute.MaxCount < 0 ? input.ReadToEnd() : input.Read(attribute.MaxCount));
        }
        // Generic collections
        else if (type.IsGenericType)
        {
            Type genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(IEnumerable<>)
                || genericTypeDefinition == typeof(IReadOnlyList<>))
            {
                return RawInputParser.ParseArray(type.GetGenericArguments()[0],
                    attribute.MaxCount < 0 ? input.ReadToEnd() : input.Read(attribute.MaxCount));
            }
            else if (genericTypeDefinition == typeof(List<>)
                || genericTypeDefinition == typeof(IList<>))
            {
                return RawInputParser.ParseList(type.GetGenericArguments()[0],
                    attribute.MaxCount < 0 ? input.ReadToEnd() : input.Read(attribute.MaxCount));
            }
        }
        return null;
    }
}
