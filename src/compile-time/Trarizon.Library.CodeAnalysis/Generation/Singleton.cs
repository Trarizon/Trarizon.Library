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

    public bool GenerateProvider { get; set; }

    public SingletonAccessibility InstanceAccessibility { get; set; }
}

public enum SingletonAccessibility
{
    Public = 0,
    Internal,
    Protected,
    Private,
    PrivateProtected,
    ProtectedInternal,
}
