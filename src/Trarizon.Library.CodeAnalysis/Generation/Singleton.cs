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
    /// <br />
    /// <see langword="null" /> if no provider needed, empty string if use default provider name
    /// </summary>
    public string? SingletonProviderName { get; set; }

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
