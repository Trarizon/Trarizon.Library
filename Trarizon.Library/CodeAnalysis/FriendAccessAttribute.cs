using System.Diagnostics;

namespace Trarizon.Library.CodeAnalysis;
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
}
