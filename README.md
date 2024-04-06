# Trarizon.Library

Private lib, Based on .NET 8, written in C#12

~some method are just for fun though~

## [Library](./src/Trarizon.Library/README.md)

Miscellaneous thing. There will be many breaking changes (orz.

<details>
<summary>brief</summary>

- CodeGenerator/Analyzers
	- MemberAccess
		- `[FriendAccess]`: restrict access of member to specified several types
		- `[BackingFieldAccess]`: A workaround of semi-auto-property, restrict access of a field to specified member
	- `[Singleton]`
	- TaggedUnion : Generate tagged union within enum type
- Wrappers: Monads, `Optional<T>`, `Result<T, TError>`, `Either<TLeft, TRight>`
- Collections:
	- Queries: More Linq-like methods for `IEnumerable<>`, `IList<>`, `IReadOnlyList<>`
	- Helpers: more extensions for BCL collection types.
	- AllocOpt: Rewrite BCL basic collection types in `struct` as light-weighted version
- Helpers: miscellaneous extensions for BCL types

</details>

## [GeneratorToolkit](./src/Trarizon.Library.GeneratorToolkit/README.md)

Toolkit for source generator

## Deprecated

<details>
<summary>Expand</summary>

### [Yieliception](./Trarizon.Yieliception/README.md) ([EN](./Trarizon.Yieliception/README.en.md))

> [Archive branch](https://github.com/Trarizon/Trarizon.Library/tree/archive_yieliception/src/Trarizon.Yieliception)

Provide interception for `I(Async)Enumerator<>.MoveNext(Async)`. 

This mainly aims to extends `yield return`, provide communication capability to
`yield return`-generated iterator. (like python

> Example: [`Test.Run.Examples.YieliceptionExample`](./Trarizon.Test.Run/Examples/YieliceptionExample.cs)

### TextCommanding

> Use [`Trarizon.TextCommand`](https://github.com/Trarizon/Trarizon.TextCommand) instead
> 
> Original project in [this branch](https://github.com/Trarizon/Trarizon.Library/tree/archive_textcommanding/Trarizon.TextCommanding)

Parse text input command(CLI like).

</details>