using System;

namespace Trarizon.Library.CodeAnalysis.Diagnostics;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class ExternalSealedAttribute : Attribute;
