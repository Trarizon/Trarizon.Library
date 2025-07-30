using System;

namespace Trarizon.Library.CodeAnalysis.Generation;
[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : Attribute
{
    /// <summary>
    /// Name of the instance property
    /// </summary>
    public string? InstancePropertyName { get; set; }

    /// <summary>
    /// Name of the genereted singleton provider,
    /// default name is <c>__SingletonProvider</c>
    /// </summary>
    /// <remarks>
    /// The value has no effect if no provider is generated
    /// </remarks>
    public string? SingletonProviderName { get; set; }

    public SingletonOptions Options { get; set; }
}

[Flags]
public enum SingletonOptions
{
    None = 0,
    /// <summary>
    /// Generate a singleton provider to keep the instance.
    /// </summary>
    /// <remarks>
    /// When use a provider, the instance will be created only when you access the instance property.
    /// Otherwise, once you access the type, instance will be created, and all other static member will all be created
    /// </remarks>
    GenerateProvider = 1,
    /// <summary>
    /// Mark the instance as internal
    /// </summary>
    IsInternalAccessibility = 1 << 1,
}