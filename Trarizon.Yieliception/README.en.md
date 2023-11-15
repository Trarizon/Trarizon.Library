# Yieliception

> `Yield + intercept = yielicept`
> 
> well I just want a short word that describes this except intercept xD

Provide interception for `I(Async)Enumerator<>.MoveNext(Async)`. 

This mainly aims to extends `yield return`, provide communication capability to
`yield return`-generated iterator. (like python

> Example [`RunTest.Examples.YieliceptionExample`](../Trarizon.Test.Run/Examples/YieliceptionExample.cs)

<details>
<summary>Types</summary>

Type|Remarks
:-:|:--
`YieliceptableIterator<>`<br/>`AsyncYieliceptableIterator<>`|Use `YI<object>` as non-type-parameter version
`YieliceptionExtensions`|Provides `ToYieliceptable` and other methods
***Yieliceptors*** |
`IYieliceptor<>`|Use `IY<object>` as non-type-parameter version, param is `null` when called as parameterless version
`TimerYieliceptor`|No interception, but will auto-`Next()` when timer elapsed
`ValidationYieliceptor<,>`<br/>`ValidationYieliceptor<>`<br/>`ConditionYieliceptor`|Validate by delegates, and also provide args
`ValueDeliveryYieliceptor<>`|Pass arguments
`ValueDeliverer<,>`|Specialized for communication
***Components*** |
`IYieliceptionComponent`<br/>`IAsyncYieliceptionComponent`|
`YieliceptionTimer`<br/>`ITimerYieliceptor`|Timer, auto-`Next()` after a period of no response or not validated.

`Yieliceptor`s are basically reusable, you can use `WithXXX()` method to reuse.

</details>

## Iterator Interception

> Example: `RunTest.Examples.YieliceptionExample`

Iterator should returns `IEnumerator<out IYieliceptor<>?>`, use `yield return` to yield interceptors.

Use `iterator.ToYieliceptable()` create interceptable iterator.

### Yieliceptor

> Implements `IYieliceptor`

When `iterator.Next()` is called, use `CanMoveNext()` to predicate whether
this resume is allowed. btw you can use this method to pass values into iterator.

if `yieliceptor` is `null`, predicate returns `true`.

### Component

> Implements `IYieliceptableComponent`

call `OnNext(iterator, bool)` when `iterator.Next()` is called
and the iterator didn't end,

## Iterator Communication

> Example [`RunTest.Examples.YieliceptionExample.RunCommunication`](../Trarizon.Test.Run/Examples/YieliceptionExample.cs)

>*Similar to Python*

Use `IEnumerator<ValueDeliverer<,>>.SendAndNext()` to communicate.
