# Trarizon.Library.Collections

## 1.0.1

- Reimplement `PrefixTree(Dictionary)` with node, the original implementation is renamed to `ContiguousPrefixTree(Dicitionary)` and marked as experimental
    - Add `AlternateLookup`
- Add `TraAlgorithm.LevenshteinDistance`
- Add `IEnumerable.Intersperse`, `Interleave`, `RepeatInterleave`, `ChunkBy`
- Add `TraCollection.Singleton`, `List.ReplaceAll`
- Add `TraComparison.CreateComparable/Equatable`
- Remove `IEnumerable.LookAhead`, remove redundant type parameter of `MinMaxOrNull`
- Remove Contiguous collections
- Add/Optimize apis of `PrefixTree`