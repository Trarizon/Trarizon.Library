# Trarizon.Library.Functional

Provides monads-related utilities.

Note that the library is not designed for pure functional programming.

## Monads

### Built-in monads

`Optional<T>`, `Result<T, TError>` are available.

For each type, there exists some related types, for example:

|Type|Description|
|---|---|
|`Optional<T>`| The monad type, is a value type
|`Optional`   | Static class providing factory methods to create a `Optional<T>`
|`RefOptional<T>`| `ref struct` supported
|`RefOptional`| Static class factory, Also provides extension for `Optional`(C#14+)

As all monads types are struct, with no inheritance, There're some tricks to simplify the use.

#### Creation

`Optional.Of<T>()` and `Optional.None` are provided for creating instance.

`Optional.None` returns `Optional.NoneBuilder`, which implicit castable to any `Optional<T>` or `RefOptional<T>`, so we can do 
``` C#
Optional<string> a = Optional.None
```
rather than manually point out the type like `Optional.None<string>()`.

If you want to specify the type of none, you can also use
```
var a = Optional<string>.None; // infer to Optional<string>
var b = Optional.None.Build<string>() // infer to Optional<string>
```

Similar apis in `Result<T, TError>`

|Optional|Result|
|---|---|
|`Optional.Of<T>(T)`|`Result.Success<T, TError>(T)`<br/>`Result.Failure<T, TError>(TError)`
|`Optional.None.Build<T>()`|`Result.Success<T>(T).Build<TError>()`<br/>`Result.Failure<TError>(TError).Build<T>()`

For `RefOptional<T>`, you can still use `Optional.Of()`, there is a extension overload for `ref struct`(C#14+), and if you want to specify to the `RefOptional<T>`, use `RefOptional.Of<T>()`

#### Translate

> The library is not designed for pure functional programming

For this reason, we don't use the method name in funcitonal theories, but use the name in LinQ

Each method has a bunch of overloads to support a fluent use experience with `ref struct`

|Method|Description|
|---|---|
|`Select`| Map
|`Where` | Filter
|`Bind`  | Bind, `SelectMany` is hidden just for linq expression
|`Match` | Match, can either return a value or `void`
|`Or`    | `this` if have value, otherwise `other`
|`Tap`   | Perform action and return self
|`Zip`   | Linq `Zip`
|`GetValueOrThrowError`| For `Result<T, TException>`, throw `TException` when error
|`Swap`  | Swap

Some of these methods are also implemented for `Optional.NoneBuilder`, `Result.SuccessBuilder`, etc.

#### Extensions

More methods use to interact with `Task<T>`, `IEnumerable<T>`, etc.

##### Async

All extension methods for `Task<T>` also available for `ValueTask<T>`

`this` Type|Method|Description|
|---|---|---|
|`Optional<Task<T>>`<br/>`Result<Task<T>, TError>`|`Transpose`|
|`Optional<Task>`<br/>`Optional<Task<T>>`|`GetAwaiter`|

##### Enumerable

methods for `IEnumerable<TMonad>`

|Method|Description|
|---|---|
|`Collect`|Collect all values in `Optional<T>`/`Result<T, TError>`, return None/Error if one of item is None/Error
|`WhereHasValue`<br/>`WhereIsSuccess`<br/>`WhereIsFailure`| `this.Where(x => x.HasValue).Select(x => x.Value)`

##### Functor

Convert between `Nullable<T>` and built-in monads

|Method|Description|
|---|---|
|`ToNullable`<br/>`ToResult`<br/>`ToOptional`|
|`Transpose`|

### Unit

As we all know that `Optional<void>` is invalid in C#

Some specialized overloads are provided for `Unit`

## Union

### Type Union Generator

`TypeUnionAttribute` is provided to generate a non-boxing type union. `ref struct` supported.

```
[TypeUnion(typeof(ReadOnlySpan<char>), typeof(string), typeof(List<string>))
partial struct MyUnion;
```

In the generated type, all reference types are overlapped, all unmanaged types are overlapped. Some methods are generated:

|Method|Description
|:--|:--
|ctor|Constructor
|implicit operateor| implicit operator cast from type to union
|`TryAs<T>`|Try get specific type instance
|`As<T>`   |Get specific type instance or default
|`Is<T>`   |Check if the instance is specific type

#### Options

- `ShareInterface`
    - `Disabled`
    - `Enabled` If all types in a union implements a same interface, the result union type will also implement it
    - `Explicit` While implementing a interface, all members are explicitly implementd
