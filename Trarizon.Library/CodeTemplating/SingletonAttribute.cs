namespace Trarizon.Library.CodeTemplating;
[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : Attribute
{
    /// <summary>
    /// Name of the instance property
    /// </summary>
    public string? InstancePropertyName { get; init; }

    /// <summary>
    /// Type name of nested provider class,
    /// default is <c>__{ClassName}_SingletonProvider</c><br/>
    /// </summary>
    /// <remarks>
    /// The value has no effect if <see cref="SingletonOptions.NoProvider"/> set
    /// </remarks>
    public string? SingletonProviderName { get; init; }

    public SingletonOptions Options { get; init; }
}

[Flags]
public enum SingletonOptions
{
    None = 0,
    /// <summary>
    /// Do not use provider to create instance
    /// </summary>
    /// <remarks>
    /// By default, we create a nested class with a public field as provider <br/>
    /// If this option set, we directly create and assign to instance property, 
    /// in which case when you use this instance, all other static fields in this
    /// type will be initialized.
    /// </remarks>
    NoProvider = 1 << 0,
}