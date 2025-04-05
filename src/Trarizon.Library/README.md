# Trarizon.Library

Miscellaneous Helpers
This library use nuget package [`CommunityToolkit.HighPerformance`](https://github.com/communitytoolkit/dotnet) and `CommunityToolkit.Diagnostics` 

Contents:

- [CodeAnalysis](#CodeAnalysis)
- [CodeGeneration](#CodeGeneration)
- [Collections and helpers](#Collections)
- [Monads and wrappers](#Wrappers)
- [Other helpers](#More)

## CodeAnalysis

- `[ExternalSealed]` : Indicates a type or interface cannot be inherited or implemented by types in another assembly
- `[BackingFieldAccess]` : a workround of .NET 9 backing field
- `[FriendAccess]` : Opt-in ver of `friend` in c++

## CodeGeneration

- `[Singleton]` : Generate a singleton class, thread safe with static field
- `[OptionalOut]` : Mark it on a `out` parameter, a method with same signature without the `out` parameter will be generated
    - Currently does not support multiple `out` parameters

## Collections

- namespace `Generic` : Extension of `System.Collections.Generic`
    - `ContiguousLinkedList<>` : Linked list, with items stored in contiguous memory
    - `IBinaryTreeNode` & `BinaryTreeNode` : Abstraction of binary tree node
    - `ILinkedListNode` & `LinkedListNode` : Abstraction of linked list node, provides some helper methods in `LinkedListNode`
    - `Deque<>` : Double-ended queue
    - `RingQueue<>` : Ring queue with fixed capacity, optional throw or overwrite when full
    - `Trie<>` : Trie
- namespace `Specialized` : 
    - `ArrayFiller<>` : helper struct to fill a array
    - `ArrayTruncation<>` : Similiar to `ArraySegment`, but always start from index 0
    - `AutoAllocList<>` : List with auto allocation and release while modifying
    - `ListDictionary<,>` : Generic version of `System.Collections.Specialized.ListDictionary`.
    - `Memento<>` : Memento pattern implementation, with `Add`, `Rollback`, `Reapply` methods
    - `SortedList<>` : List with items sorted
- namespace `StackAlloc` : `ref struct` collections
    - `(ReadOnly)ConcatSpan` : Concat 2 spans
    - `(ReadOnly)ReversedSpan` : Reversed span
    - `StackAllocBitArray` : Bit array using a `Span<byte>` as underlying items
- `TraXXX` : Static classes with helper method, extension methods
    - `TraAlgorithm` : 
    - `TraArray` : Helper for `T[]`, `ImmutableArray<>`
    - `TraComparison` : Helpers for `IComaprer<>`, `IComparable`, etc.
    - `TraDictionary` : Helper for `Dictionary<,>`
    - `TraEnumerable` : Helper for `IEnumerable`, linq extensions
    - `TraIter` : Value type simple Linq, this class is design for instant iterating, so it doesn't support chain linq
    - `TraList` : Helpers for `List<>`, `I(ReadOnly)List<>`
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
    - `WithIndex` : Yield index and item, `Index` in .NET 9
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
<summary>Iter</summary>

These methods are implements for instant iteration, so all iterators are implements with `struct`,
but not implements `IEnumerable<>` or `IEnumerator`

Too lazy to implement all linqs, so I'll just implement what I have used.

All extensions methods identifiers are start with `Iter`

- Creation
    - `IterateByWhile/NotNull` : Yield next value selected by a `Func<T, T>`, until predicate failed
    - `Range` : Enumerate `int` from `start` to `end`(not include)
    - `RangeTo` : Iterate `int` from 0 to `count` with specific step
- Mapping
    - `WithIndex`
- Sorting
    - `Reverse` : `Reverse` in Linq will always cache values in collection, this won't do that because designing for instant iteration

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

## Wrappers

- `Either<,>` : Monad either
- `LazyInitDisposable` : Wrapper for fully utilizing the `using` statement when lazy-init `IDisposable` objects.
- `Result<,>` : Monad Result, for smaller size, `TError` only supports reference type, and if `TError` is null, the result means success
- `Optional<>` : Monad Option

## More

The namespace structure is almost the same with `System.XXX`

- namespace `Buffer`
    - `ObjectPool` : Object pool
    - `IArrayAllcator` : Interface for array allocator
        - `ArrayPool<>.Rent` : Extension method for `ArrayPool`, returns a auto-return-available wrapper with `using` statement
    - `IObjectAllcator` : Interface for object allocator
- namespace `Components`
    - `IFlagNotifiable` : Interface for notification, a cheaper and stricter `INotifyPropertyChanged`
        - `FlagNotifiable` : Extensions and static methods for global notification
        - `FlagNotifiable<>/<,>` : Abstract classes that implements `IFlagNotifiable` and provide a method `InvokeNotification`, not thread-safe
        - `FlagNotifier` : Designed as a field to help implements `IFlagNotifiable`
- namespace `IO`
    - `TraPath` : Extends `System.IO.Path`
    - `TraStream` : Helpers for `System.IO.Stream`
- namespace `Numerics`
    - `BoundedInterval` : Represents a interval 
    - `Interval` : Represents a left-close, right-open interval
    - `Ray2D/3D` : Ray
    - `TraGeometry` : Helpers for `Vector2/3`, `Quaternion`, etc.
    - `TraNumber` : Helpers for number types (in `System.Numerics`), `Index`, `Range`
- namespace `Text`
    - `TraString` : Helper for `string`
    - namespace `Json`
        - `WeakJsonElement` : Wrapper of `JsonElement` that auto check `JsonValueKind`
- namespace `Threading`
    - `AsyncSemaphoreLock` : Async lock implemented with `SemaphoreSlim`
    - `TraAsync` : Helpers for async operation, `Task<>`, `ValueTask<>`, etc.
- `TraDelegate` : Helpers for delegates
- `TraEnum` : Helpers for enum types
- `TraRandom` : Helpers for `Random`
- `TraTuple` : Helpers for `ValueTuple`s
- `TraUnsafe` : Extends `Unsafe`

### Helpers

- ArrayPool
    - `Rent` : Overload for `ArrayPool<>.Rent`, returns a auto-return object avaible with `using` statement
- Path
    - `IsValidFile/PathName` : check validation of file/path name, with `SearchValues`
    - `ReplaceInvalidFileNameChar` : Replace invalid file name characters
- Stream
    - `Read(Exactly)` : Read data into `unmanaged` span
    - `ReadExactlyIntoArray` : Read exactly data into an `unmanaged` array with specific length
    - `ReadWithInt32Prefix` : Read a `int` as array length, and do `ReadExactlyIntoArray`
- Number
    - `IncAnd(Try)Wrap` : Increment the number, if the result is greater than given `max`, then wrap it
    - `Normalize` : Linear normalize value into [0,1]
    - `NormalizeUnclamped` : Linear normalize value into [0,1], but not clamped
    - `MapTo` : Linear map a value from [a, b] to [c, d], no clamp
    - `FlipNegative` : `if (value < 0) value = ~value`, useful on `BinarySearch` result
    - `Min/Max` : Overloads for `params ReadOnlySpan<>`
    - `MinMax(T, T)` : Reorder input 2 args
    - `MinMax(ROS<T>)` : Get min and max in one iteration
    - `GetCheckedOffset` : `Index.GetOffset` with overflow check
    - `ValidateSliceArgs` : Check if `start` and `length` is valid within a collection
- Geometry
    - `ToNormalized` : Normalize `Vector2/3`, `Quaternion`
    - `ToEulerAngles`
- Async
    - `GetAwaiter` : Support `await` keyword for `ValueTask?`, `ValueTask<>?`
    - `CatchCancallation` : Return a awaitable that will catch `TaskCancellationException`.
- Delegate
    - `Create` : Create delegate with an `object` and a static method. It is actually the way compiler use to create delegate for extension methods.
- Enum
    - `HasAnyFlag` : Check if a enum value has one of given flags.
- Random
    - `SelectWeight` : Weighted random
    - `NextSingle/Double` : Get a random float number in specific range
    - `NextBoolean` : Get a random boolean value
    - `NextItem` : Get a random item in collection
- Tuple
    - `ToKeyValuePair` : Convert pair to k-v pair
- Unsafe
    - `AsReadOnly` : Perform `Unsafe.As` for `ref readonly` variables
- Utils
    - `SetField` : If given value is not equals to field, set value and return `true`, otherwise return `false`
