# History

## v0.3.0 (in progress)

- Breaking
    - Add `GetValidValue` for monads
    - Monad.`Value`s will not throw if unset now
    - Moved `ReversedSpan`
    - Replace `ReversedSpan.OriginalSpan` with `Reverse()`
    - Rename `FriendAttribute` to `FriendAccessAttribute`
    - Combine namespace `Collections.Creator` and `Collections.Extensions` to `Collections.Helpers`
    - Rename `Extension`s to `Helper`s
- New
    - Add Monad.`GetValueRefOrDefaultRef()`
    - Add `NotNull<T>`
    - Add `IList<>.Adjacent()`
    - Add `IList<>.Select()` indexed version
    - Add `IList<>.CartesianProduct()`
    - Add `ValueTask<>?.GetAwaiter()`
    - Add `ValueTask<>?.Sync()`
- Changes
    - Optimize `IEnumerable<>.CartesianProduct()`
    - Add zero-length check for some list queries
- Bugs
    - Fix bug when use array type in `[FriendAccess]`, analyzer will crash. (though maybe nobody will do this

## v0.2.2

- New
    - Add more collection members to `StackAllocBitArray`
    - Add `ReversedSpan<>.CopyTo`
    - Add overload method `IEnumerable<>.StartWith()` for `T[]`
    - Add `IEnumerable<>.CartesianProduct`
    - Add `FriendAttribute`
    - public `StableSortComparer<>` and provide new overloads
- Changes
    - Optimize `AllocOptSet<>` with `StackAllocBitArray`
    - Optimize `IEnumerable<>.TrySingle()`, `TryFirst()`
    - Remove unused method in `IKeyedEqualityComparer`(internal)
    - Optimize internal structure of `AllocOptSet<>`

## v0.2.1

- New
    - Add `AllocOptDictionary<,>`, `AllocOptList<>`, `AllocOptSet<>`, `AllocOptStack<>`
    - Add `RepeatForever()`
    - Add `Either<,>.ToString()`
    - Add `ReversedSpanQuerier<>.Slice()` and `ToArray()`
    - Add `T[].OffsetOf()`
    - Add `IEnumerable<>.ForEach()`
    - Add `StackAllocBitArray`
- Breaking
    - Remove `ArrayFiller<>`, use `AllocOptList<>` instead
    - Adjust signature of `IEnumerable<>.Merge` and `IEnumerable.IsInOrder`
    - Rename `Span<>.Reverse` to `ToReversedSpan`
    - Move `Span<>.Reverse` to `SpanExtensions`
- Changes
    - Change format of `Optional<>.ToString()`, `Result<,>.ToString()`, and they won't return `null` now
    - Optimize nullable analysis for `IEnumerable<>.TrySingle()`, `TrySingleOrNone()`, `PopFirst()`
- Bug fix
    - Order judge in `IEnumerable<>.Merge`

## v0.2.0

- New
    - Add conversion between wrappers
    - Add `ArrayFiller<>`
    - Add `Either<,>`
    - Add `IList<>.TryAt()`，`IROList<>.TryAt()`
    - Add `Optional<>.Unwrap`
- Changes
    - Use Exception static methods in .NET 8 to replace part of `ThrowHelper`
- Breaking
    - Remove target framework .NET Standard 2.0 (~~反正源生成器不能用，不如等什么时候源生成器支持新版.NET吧.jpg~~
    - `Span.Reverse` is moved to `SpanQuery`
    - Remove `IList<>.AtOrDefault()`
    - Rename `IReadOnlyList<>.AtOrDefault()` to `ElementAtOrDefault`
    - Modified signature of `IEnumerable<>.Merge`
- Bug fix
    - Exception judge in `IEnumerable.CountsBetween`
