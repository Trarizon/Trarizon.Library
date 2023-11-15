namespace Trarizon.TextCommanding.Attributes;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class TextCommandAttribute(string name, SubCommandOption subCommandOption, params Type[] explicitSubCommands)
    : Attribute
{
    public string Name => name;
    public SubCommandOption SubCommandOption => subCommandOption;
    public Type[] ExplicitSubCommands => explicitSubCommands;

    public TextCommandAttribute(string name, params Type[] explicitSubCommands) :
        this(name, SubCommandOption.OnlyExplicitTypes, explicitSubCommands)
    { }

    public TextCommandAttribute(string name):
        this(name, SubCommandOption.IncludingNestedTypes)
    { }
}
