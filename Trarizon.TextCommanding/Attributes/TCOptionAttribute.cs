namespace Trarizon.TextCommanding.Attributes;
public sealed class TCOptionAttribute(string fullName, string? shortName = null) : TCParameterAttribute
{
    public string FullName => fullName;
    public string? ShortName => shortName;

    internal string DisplayName => shortName is null
        ? fullName
        : $"{fullName}|{shortName}";
}
