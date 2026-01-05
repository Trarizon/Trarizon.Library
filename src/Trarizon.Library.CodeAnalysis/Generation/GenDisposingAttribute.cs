using System;

namespace Trarizon.Library.CodeAnalysis.Generation;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class GenDisposingAttribute : Attribute
{
    public bool Explicitly { get; set; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class GenDisposingIgnoreAttribute : Attribute { }
