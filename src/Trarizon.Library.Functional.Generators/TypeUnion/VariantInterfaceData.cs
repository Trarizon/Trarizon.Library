using Microsoft.CodeAnalysis;
using Trarizon.Library.Roslyn.Pipeline.Collections;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

public enum UnionShareInterfaceOption { Disabled, Enabled, Explicit, }

sealed record VariantInterfaceData(
    string TypeFQName,
    SequenceEquatableImmutableArray<VariantInterfaceMemberData> Members
);

record struct VariantInterfaceMemberData(
    string Name,
    InterfaceMemberKind Kind,
    bool IsStatic,
    string ReturnTypeFQNameWithModifiers
)
{
    public bool HasGetOrAddAccessor { get; init; }
    public bool HasSetOrRemoveAccessor { get; init; }
}

record struct ParameterInfo(
    RefKind RefKind,
    string TypeFQNameWithModifiers,
    string Name
);

enum InterfaceMemberKind { Invalid, Property, Indexer, Event, Method };
