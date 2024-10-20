# Trarizon.Library

Private lib, Based on .NET 8, written in C#12

~some method are just for fun though~

## [Library](./src/Trarizon.Library/README.md)

Miscellaneous thing. There will be many breaking changes (orz.

Some methods depends on [`CommunityToolkit`](https://github.com/CommunityToolkit/dotnet)

<details>
<summary>brief</summary>

- CodeAnalysis/Generation
- Collections: Extends `System.Collections`
- Wrappers: Monads
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