# Trarizon.Library

Private lib, Based on .NET 8, written in C#12

~some method are just for fun though~

> Some examples in `Test.Run.Examples` and `Test.UnitText`
>
> Most of methods has English comments in source code,
> and Chinese `README.md` doc

## [Library](./Trarizon.Library/README.md)

Miscellaneous thing like `Optional<T>`, LinQ extensions, etc. 

Partially support .NET Standard 2.0

## [GeneratorToolkit](./Trarizon.Library.GeneratorToolkit/README.md)

Toolkit for source generator

## [TextCommanding](./Trarizon.TextCommanding/README.md)

Parse text input command(CLI like).

## [Yieliception](./Trarizon.Yieliception/README.md) ([EN](./Trarizon.Yieliception/README.en.md))

Provide interception for `I(Async)Enumerator<>.MoveNext(Async)`. 

This mainly aims to extends `yield return`, provide communication capability to
`yield return`-generated iterator. (like python

> Example: [`Test.Run.Examples.YieliceptionExample`](./Trarizon.Test.Run/Examples/YieliceptionExample.cs)
