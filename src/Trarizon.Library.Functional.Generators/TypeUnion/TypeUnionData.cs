using Trarizon.Library.Roslyn.Pipeline;
using Trarizon.Library.Roslyn.Pipeline.Collections;

namespace Trarizon.Library.Functional.Generators.TypeUnion;

sealed record TypeUnionData(
    string FileHintName,
    TypeHierarchyInfo TypeHierarchy,
    SequenceEquatableImmutableArray<VariantData> Variants
);

