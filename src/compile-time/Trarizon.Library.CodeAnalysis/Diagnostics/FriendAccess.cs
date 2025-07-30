using System;
using System.Diagnostics;

namespace Trarizon.Library.CodeAnalysis.Diagnostics;
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
