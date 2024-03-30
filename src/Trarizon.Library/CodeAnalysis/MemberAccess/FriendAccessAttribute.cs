using System.Diagnostics;

namespace Trarizon.Library.CodeAnalysis.MemberAccess;
[AttributeUsage(
    AttributeTargets.Constructor |
    AttributeTargets.Method |
    AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Event)]
[Conditional("CODE_ANALYSIS")]
public sealed class FriendAccessAttribute(params Type[] friendTypes) : Attribute
{
    public Type[] FriendTypes { get; } = friendTypes;

    public FriendAccessOptions Options { get; init; }
}

[Flags]
public enum FriendAccessOptions
{
    None = 0,
    /// <summary>
    /// Allow allow derived types of given type to access
    /// </summary>
    AllowInherits = 1,
}