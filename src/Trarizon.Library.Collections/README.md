# Trarizon.Library.Collections

Helpers for BCL Array, `Span` and collections. And more collections type.

## List

- namespace `AllocOpt`
    - `AllocOptList` : Value type list use pooled array as underlying array
- namespace `Buffers`
    - `IArrayAllocator` : Array allocator abstract, also provide a `T[].Rent(int, out T[])` method for `ArrayPool<>`
- namespace `Generic` : Extension of `System.Collections.Generic`
    - `ContiguousLinkedList<>` : Linked list, with items stored in contiguous memory
    - `Deque<>` : Double-ended queue
    - `IBinaryTreeNode` & `BinaryTreeNode` : Abstraction of binary tree node
    - `ILinkedListNode` & `LinkedListNode` : Abstraction of linked list node, provides some helper methods in `LinkedListNode`
    - `RingQueue<>` : Ring queue with fixed capacity, optional throw or overwrite when full
- namespace `Specialized` : 
    - `ListDictionary<,>` : Generic version of `System.Collections.Specialized.ListDictionary`.
    - `Memento<>` : Memento pattern implementation, with `Add`, `Rollback`, `Reapply` methods
    - `PrefixTree<>/PrefixTreeDictionary<,>` : Prefix tree
    - `SortedList<>` : List with items sorted
- namespace `StackAlloc` : `ref struct` collections
    - `(ReadOnly)ConcatSpan` : Concat 2 spans
    - `(ReadOnly)ReversedSpan` : Reversed span
    - `StackAllocBitArray` : Bit array using a `Span<byte>` as underlying items
- `TraXXX` : Static classes with helper method, extension methods
    - `TraAlgorithm` : 
    - `TraArray` : Helper for `T[]`, `ImmutableArray<>`
    - `TraCollection` : Helpers for BCL collections
    - `TraComparison` : Helpers for `IComaprer<>`, `IComparable`, etc.
    - `TraEnumerable` : Helper for `IEnumerable`, linq extensions
    - `TraIndex` : Helpers for `Index`, `Range`, and `int` when it as index
    - `TraSpan` : Helpers for `(ReadOnly)Span<>`

### Helpers

<details>
<summary>Array</summary>

- `AsEnumerable(OrNull)` for `ImmutableArray<>` : Return underlying array as `IEnumerable<>` to avoid boxing and get performance improvements with LinQ. The BCL overloaded some linq method but not all(and my own linq extensions do not support)
- `MoveTo` : Move item on `fromIndex` to `toIndex`
- `EmptyIfDefault` for `ImmutableArray<>` : Return empty array if source is `null` 
- `TryAt` for `ImmutableArray<>`

</details>

<details>
<summary>Collection</summary>

- `Dictionary`
    - `AtRef`
    - `GetOrAdd`
    - `AddOrUpdate`
- `List`
    - `GetUnderlyingArray` : 
    - `RemoveAt/RemoveRange` : overload for `Index` and `Range`
    - `MoveTo` : Move item(s) on `fromIndex` to `toIndex`
    - `GetLookup` : Returns a view treating the list as a set
    - `GetSortedModifier` : Returns a view through which modifying the list will keep elements in order.
- `Stack`
    - `AsSpan` : As `ReadOnlyReversedSpan`

</details>

<details>
<summary>Comparison</summary>

- `Reverse` : Reverse a `IComparer<>`

</details>

<details>
<summary>Enumerable</summary>

- Aggregation
    - `CountsMoreThan/LessThan/AtMost/AtLeast/EqualsTo/Between` : Judge size of collection
    - `IsDistinct(By)` : Check if the collection doesn't contains duplicate element
    - `IsInOrder(By)` : Check if the elements in collection is in order
    - `MinMax(By)` : Get minimun value and maximun value in one iteration
- Creation
    - `EnumerateByWhile/NotNull` : Yield next value selected by a `Func<T, T>`, until predicate failed
    - `EnumerateDescendantsDepth/BreadthFirst` : Traverse tree
- Element
    - `TryAt` : `TryXXX` version of `ElementAt`
    - `TryFirst` : `TryXXX` version of `First`
    - `TryLast` : `TryXXX` version of `Last`
    - `FirstNearToMax(By)(OrDefault)` : Find the first item has priority greater than given priority, if not found, return the first item with greatest priority
    - `TrySingle` : Returning tagged union version of `Single`
- Filtering
    - `Duplicates` : Return all elements that is not distinct in collection
    - `OfNotNull` : Filter out all `null` values
    - `TakeEvery` : Yield the values in specific interval
- Joining
    - `CatesianProduct` : Catesian product
    - `Merge` : Merge 2 sorted collections
- Mapping
    - `Adjacent` : Yield the value and its next value
    - `AggregateSelect` : `Aggregate` and returns all values in processing
    - `ChunkPair/Triple` : Returning tuple version of `Chunk`
    - `WithIndex` : Yield return index and item, `Index` in .NET 9
    - `LookAhead` : Return item and how many items in the sequence rest after the item, with a given max ahead count
    - `Repeat` : Repeatly enumerate the collection
- Partition
    - `OfTypeWhile` : Take values until doesn't match the given type
    - `OfTypeUntil` : Take values until reach element in given type
    - `PopFront` : Split the collection into 2 parts, the first parts is return by `out` paramter
    - `PopFirst` : Get the first element, and returns the rest elements.
- Sorting
    - `LazyReverse` : Reverse, no cache if source is `IList<T>`
    - `Rotate` : Split the collection into 2 parts and swap them
- ToCollections
    - `EmptyIfNull` : Return empty collection if source collection is `null`
    - `TryToNonEmptyList` : If collection is not empty, then collect items into `List<>`, in one iteration

</details>

<details>
<summary>Span</summary>

- Creation
    - `As(ReadOnly)Bytes` : Convert an `unmanaged` value into bytes
- Index
    - `OffsetOf` (`DangerousOffsetOf`) : Get the index of element by pointer substraction
    - `Find`/`FindIndex` :
    - `FindLower/UppderBoundIndex` : find the lower/upper bound in a sorted span
    - `LinearSearch(FromEnd)` : Linear search, similar to `BinarySearch`, returns `~index` when not found
- Modifications
    - `MoveTo` : Move item(s) on `fromIndex` to `toIndex`
- Views
    - `AsReversed` : return `(ReadOnly)ReversedSpan` of the span

</details>
