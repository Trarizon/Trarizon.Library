# Trarizon.Library

Private lib, Based on .NET 8, written in C#12

~some method are just for fun though~

> Some examples in `Test.Run.Examples` and `Test.UnitText`
>
> Most of methods has English comments in source code,
> and Chinese `README.md` doc

## [Library](./trarizon.library/readme.md)

Miscellaneous

## [TextCommanding](./trarizon.textcommanding/readme.md)

Parse text input command(CLI like).

## [Yieliception](./trarizon.yieliception/readme.md) ([EN](./trarizon.yieliception/readme.en.md))

Provide interception for `I(Async)Enumerator<>.MoveNext(Async)`. 

This mainly aims to extends `yield return`, provide communication capability to
`yield return`-generated iterator. (like python

> Example: [`Test.Run.Examples.YieliceptionExample`](./trarizon.test.run/examples/yieliceptionexample.cs)
