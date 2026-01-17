using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Trarizon.Library.Roslyn.Collections;
using Trarizon.Library.Roslyn.SourceInfos;

namespace Trarizon.Library.Functional.SourceGeneration.TypeUnion;

partial class TypeUnionGenerator
{
    record struct ParseContext(
        StructDeclarationSyntax Syntax,
        INamedTypeSymbol Symbol,
        AttributeData Attr,
        ImmutableArray<ITypeSymbol> VariantTypes);

    sealed record class UnionData(
        string FileHintName,
        TypeHierarchyInfo TypeHierarchy,
        string TypeName,
        string FullyQualifiedTypeName,

        ShareInterfaceOption ShareInterface,
        SequenceEquatableImmutableArray<(string FullQualifiedTypeName, SequenceEquatableImmutableArray<InterfaceMemberInfo> Methods)> Interfaces,
        SequenceEquatableImmutableArray<VariantData> Variants);

    sealed record class VariantData(
        int Id,
        string FullyQualifiedTypeName,
        VariantTypeKind TypeKind)
    {
        // Id of unmanaged or managed field
        public int FieldId { get; init; }
        public bool IsRefLikeType { get; init; }
        public bool IsInterface { get; init; }
    }

    record struct InterfaceMemberInfo(
        InterfaceMemberKind Kind,
        bool IsStatic,
        string ReturnTypeWithModifiers,
        string Name)
    {
        public bool ReturnsVoid { get; init; } = true;
        public RefKind ReturnRefKind { get; init; }
        public bool HasGetOrAddAccessor { get; init; }
        public bool HasSetOrRemoveAccessor { get; init; }
        public SequenceEquatableImmutableArray<string> TypeParameters { get; init; }
        public SequenceEquatableImmutableArray<ParameterInfo> Parameters { get; init; }
        public string? Constraints { get; init; }
    }

    record struct ParameterInfo(
        RefKind RefKind,
        string FullyQualifiedTypeNameWithModifiers,
        string Name)
    {
    }

    enum VariantTypeKind { Reference, Managed, Unmanaged }

    enum InterfaceMemberKind { Invalid, Property, Indexer, Event, Method }

    enum ShareInterfaceOption { Disabled, Enabled, Explicit }
}
