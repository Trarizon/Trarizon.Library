namespace Trarizon.TextCommanding.Attributes;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = true)]
public abstract class TCParameterAttribute : Attribute
{
    /// <summary>
    /// Default value when no argument set
    /// </summary>
    public object? Default { get; set; }
}
