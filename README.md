# Trarizon.Library

Private lib, Based on .NET 8, written in C#12

~some method are just for fun though~

## [Library](./Trarizon.Library/README.md)

Miscellaneous thing. There will be many breaking changes (orz.

<details>
<summary>brief</summary>

- CodeAnalysis
	- [Use generator] `FriendAccessAttribute`: like `friend` in c++
- Monads: `Optional<T>`, `Result<T, TError>`, `Either<TLeft, TRight>`
- Collections:
	- Queries: More Linq-like methods for `IEnumerable<>`, `IList<>`, `IReadOnlyList<>`
	- Extensions: more extensions for BCL collection types.
	- AllocOpt: Rewrite BCL basic collection types in `struct`, designed for one-time use in method 
- Extensions: miscellaneous extensions for BCL types

</details>

## [GeneratorToolkit](./Trarizon.Library.GeneratorToolkit/README.md)

Toolkit for source generator

## [Yieliception](./Trarizon.Yieliception/README.md) ([EN](./Trarizon.Yieliception/README.en.md))

Provide interception for `I(Async)Enumerator<>.MoveNext(Async)`. 

This mainly aims to extends `yield return`, provide communication capability to
`yield return`-generated iterator. (like python

> Example: [`Test.Run.Examples.YieliceptionExample`](./Trarizon.Test.Run/Examples/YieliceptionExample.cs)

## Deprecated

<details>
<summary>Expand</summary>

### TextCommanding

> Use [`Trarizon.TextCommand`](https://github.com/Trarizon/Trarizon.TextCommand) instead
> 
> Original project in [this branch](https://github.com/Trarizon/Trarizon.Library/tree/archive_textcommanding/Trarizon.TextCommanding)

Parse text input command(CLI like).

</details>