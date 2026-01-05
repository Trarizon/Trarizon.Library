namespace Trarizon.Library.Roslyn.SourceInfos;

public sealed class TypeNameInfo(string typeMetadataName)
{
    public static readonly TypeNameInfo GeneratedCodeAttribute = new("System.CodeDom.Compiler.GeneratedCodeAttribute");

    public string MetadataName { get; } = typeMetadataName;

    /// <summary>
    /// The namespace of the type
    /// <br/>
    /// <c>System.Collections.Generic</c> for <c>System.Collections.Generic.List`1</c>
    /// </summary>
    public string Namespace
    {
        get {
            if (field is null) {
                var idx = MetadataName.LastIndexOf('.');
                field = idx == -1 ? string.Empty : MetadataName[..idx];
            }
            return field;
        }
    }

    /// <summary>
    /// The name of the type
    /// <br/>
    /// <c>List</c> for <c>System.Collections.Generic.List`1</c>
    /// </summary>
    public string Name
    {
        get {
            if (field is null) {
                var genericIdx = MetadataName.IndexOf('`');
                var endIdx = genericIdx == -1 ? ^0 : genericIdx;
                var idx = MetadataName.LastIndexOf('.');
                var startIdx = idx == -1 ? 0 : idx + 1;
                field = MetadataName[startIdx..endIdx];
            }
            return field;
        }
    }

    /// <summary>
    /// The name of the type with arity
    /// <br/>
    /// <c>List`1</c> for <c>System.Collections.Generic.List`1</c>
    /// </summary>
    public string NameWithArity
    {
        get {
            if (field is null) {
                var idx = MetadataName.LastIndexOf('.');
                var startIdx = idx == -1 ? 0 : idx + 1;
                field = MetadataName[startIdx..];
            }
            return field;
        }
    }

    /// <summary>
    /// The full name of the type
    /// <br/>
    /// <c>System.Collections.Generic.List</c> for <c>System.Collections.Generic.List`1</c>
    /// </summary>
    public string FullName
    {
        get {
            if (field is null) {
                var genericIdx = MetadataName.IndexOf('`');
                var endIdx = genericIdx == -1 ? ^0 : genericIdx;
                field = MetadataName[..endIdx];
            }
            return field;
        }
    }

    public int Arity
    {
        get {
            if (field == -1) {
                var idx = MetadataName.IndexOf('`');
                field = idx == -1 ? 0 : int.Parse(MetadataName[(idx + 1)..]);
            }
            return field;
        }
    } = -1;
}
