# Trarizon.Library.Functional

Provides utilities for functional programming (Monads)

## Monads

`Optional<>`, `Result<,>`, `Either<,>` are available, a non-generic type with same name is provided as factory(eg: `Optional` for `Optional<T>`, etc.).

All monads are value type

### `Optional` & `RefOptional`

Use `Optional.Of` / `Optional.None` / `Optional<T>.None` to create a `Optional<T>` with or without value. 
We provider `Optional.Create/OfNotNull/OfPredicate` to help you quickly create a `Optional<T>` according to some predicate

With `allows ref struct` in .NET9, `RefOptional<T>` is provided as a `Optional<T>` for `ref struct` and has almost same functions with `Optional<T>`.
You can use `RefOptional.Of` etc. to create `RefOptional<T>`, if your language version is C#14 or later, you can just use `Optional.Of`, and `RefOptional<T>` will be created if argument is `ref struct`.

`Optional<T>` and `RefOptional<T>` are implicitly convertable to each other, you can also use `AsRef()` and `AsDeref()` to convert between `Optional<T>` and `RefOptional<T>`

`Optional<T>` is implicited castable from a value

`Optional<T>` has following members to access data
- `HasValue`
- `Value` : This property won't throw if this is `None`
- `GetValueOrThrow()`
- `GetValueOrDefault()`, `GetValueOrDefault(T)`
- `TryGetValue(out T)`
- `GetValueRefOrDefaultRef()`

`Optional<T>` has following members to chain
- `operator |` or `operator ||`
- `Or()`
- `Cast<TResult>()` so you can cast it to its interfaces of base type
- `Match()` return `TResult` or `void`
    - `MatchValue()` and `MatchNone`
- `Select()` map value
- `Bind()` map value to `Optional<TResult>`
    - `SelectManay()` is also provided, so you can use linq expression to do this
- `Zip()`
- `Where()` filter
- `ToNullable()` convert to `Nullable<T>`

`Optional<T>` has some methods related to `IEnumerable<T>`
- `IEnumerable<Optional<T>>.Collect(): Optional<IEnumerable<T>>` returns the sequence if all optionals has value, else return `None`
- `IEnumerable<T>.WhereSelect(Func<T, Optional<TResult>>) : IEnumerable<TRsult>` Linq `Where` and `Select`
- `IEnumerable<Optional<T>>.WhereHasValue() : IEnumerable<T>` returns sequence contains values


You may notice that `Optional.None` returns a `Optional.NoneBuilder`, `Result.Success()` returns a `Result.SuccessBuilder<T>` etc.
They can be implicitly converted to or use `Build()` to create a monad type.

I use this so that when I want to create a `Optional.None`, I don't have to specify the type of value like `Optional.None<T>()`,
this works better on `Result<T, TError>`, as you can use `Result.Success(1)` to create a `Success` rather than `Result.Success<int, string>(1)`, C# force you to write all type arguments if compiler don't know.

Also, all types are designed in value type, so I should avoid boxing and cannot use inheritance

### Result
