using Trarizon.TextCommanding.Input.Parameters.CommandParameters;

namespace Trarizon.TextCommanding.Definition;
internal interface IOptionParameterInfoProvider
{
    IReadOnlyDictionary<string, OptionParameter> ViaFullName { get; }
    IReadOnlyDictionary<string, OptionParameter> ViaShortName { get; }
}
