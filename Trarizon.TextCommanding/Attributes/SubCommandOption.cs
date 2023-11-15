namespace Trarizon.TextCommanding.Attributes;
public enum SubCommandOption
{    /// <summary>
    /// Only types explicitly added to <see cref="CommandSetAttribute"/> will treat as sub commands
    /// </summary>
    OnlyExplicitTypes,
    /// <summary>
    /// All nested types that decorated with <see cref="CommandSetAttribute"/>
    /// </summary>
    IncludingNestedTypes,
}
