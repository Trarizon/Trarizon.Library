using System;

namespace Trarizon.Library.SourceGenerator.Toolkit;
[Flags]
public enum AccessModifiers
{
    None = 0,
    Public = 1 << 0,
    Protected = 1 << 1,
    Internal = 1 << 2,
    ProtectedInternal = Protected | Internal,
    Private = 1 << 3,
    PrivateProtected = Private | Protected,
    File = 1 << 4,
}
