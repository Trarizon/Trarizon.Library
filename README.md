# Trarizon.Library

Personal library, supports .NET 8/9, .NET Standard 2.0/2.1

~some method are just for fun though~

## [Library](./src/Trarizon.Library/README.md)

Miscellaneous stuff. Breaking changes may be frequent (orz.

Some methods depends on [`CommunityToolkit`](https://github.com/CommunityToolkit/dotnet)

<details>
<summary>brief</summary>

- CodeAnalysis/Generation
- More collections, more extensions
- Simple struct monads
- Many utility methods extends from BCL

</details>

## GeneratorToolkit

Toolkit for source generator

<details>
<summary>Deprecated</summary>

## Deprecated

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