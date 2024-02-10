# History

## v0.2.1 (in progress)

- New
    - Add `AllocOptDictionary<,>`, `AllocOptList<>`, `AllocOptSet<>`, `AllocOptStack<>`
    - Add `RepeatForever()`
    - Add `Either<,>.ToString()`
    - Add `ReversedSpanQuerier<>.Slice()` and `ToArray()`
- Breaking
    - Remove `ArrayFiller<>`, use `AllocOptList<>` instead
    - Adjust signature of `IEnumerable<>.Merge` and `IEnumerable.IsInOrder`
    - Rename `Span<>.Reverse` to `ReverseSpan`, to distinguish from `Reverse` method in BCL
- Changes
    - Change format of `Optional<>.ToString()`, `Result<,>.ToString()`, and they won't return `null` now
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
