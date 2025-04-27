# Trarizon.Library

Miscellaneous Helpers
This library use nuget package [`CommunityToolkit.HighPerformance`](https://github.com/communitytoolkit/dotnet) and `CommunityToolkit.Diagnostics` 

Contents:

- [CodeAnalysis](#CodeAnalysis)
- [CodeGeneration](#CodeGeneration)
- [Collections and helpers](#Collections)
- [Monads and wrappers](#Wrappers)
- [Other helpers](#More)

## CodeAnalysis

- `[ExternalSealed]` : Indicates a type or interface cannot be inherited or implemented by types in another assembly
- `[BackingFieldAccess]` : a workround of .NET 9 backing field
- `[FriendAccess]` : Opt-in ver of `friend` in c++

## CodeGeneration

- `[Singleton]` : Generate a singleton class, thread safe with static field
- `[OptionalOut]` : Mark it on a `out` parameter, a method with same signature without the `out` parameter will be generated
    - Currently does not support multiple `out` parameters

## Wrappers

- `Either<,>` : Monad either
- `LazyInitDisposable` : Wrapper for fully utilizing the `using` statement when lazy-init `IDisposable` objects.
- `Result<,>` : Monad Result, for smaller size, `TError` only supports reference type, and if `TError` is null, the result means success
- `Optional<>` : Monad Option

## More

The namespace structure is almost the same with `System.XXX`

- namespace `Buffer`
    - `ObjectPool` : Object pool
    - `IObjectAllcator` : Interface for object allocator
- namespace `Components`
    - `EventBus` : A event bus implementation
    - `IFlagNotifiable` : Interface for notification, a cheaper and stricter `INotifyPropertyChanged`
        - `FlagNotifiable` : Extensions and static methods for global notification
        - `FlagNotifiable<>/<,>` : Abstract classes that implements `IFlagNotifiable` and provide a method `InvokeNotification`, not thread-safe
        - `FlagNotifier` : Designed as a field to help implements `IFlagNotifiable`
- namespace `IO`
    - `TraPath` : Extends `System.IO.Path`
    - `TraStream` : Helpers for `System.IO.Stream`
- namespace `Mathematics`
    - `BoundedInterval` : Represents a interval 
    - `Interval` : Represents a left-close, right-open interval
    - `Rational` : Rational number
    - `TraGeometry` : Helpers for `Vector2/3`, `Quaternion`, etc.
    - `TraIndex` : Helpers for `Index`, `Range`
    - `TraNumber` : Helpers for number types (in `System.Numerics`)
    - `Geometry2D`/`Geometry3D`
        - `Line2D`
        - `LineSegment2D`
        - `Ray2D/3D`
- namespace `Text`
    - `TraString` : Helper for `string`
    - namespace `Json`
        - `WeakJsonElement` : Wrapper of `JsonElement` that auto check `JsonValueKind`
- namespace `Threading`
    - `AsyncSemaphoreLock` : Async lock implemented with `SemaphoreSlim`
    - `TraAsync` : Helpers for async operation, `Task<>`, `ValueTask<>`, etc.
- `TraDelegate` : Helpers for delegates
- `TraEnum` : Helpers for enum types
- `TraRandom` : Helpers for `Random`
- `TraTuple` : Helpers for `ValueTuple`s
- `TraUnsafe` : Extends `Unsafe`

### Helpers

- ArrayPool
    - `Rent` : Overload for `ArrayPool<>.Rent`, returns a auto-return object avaible with `using` statement
- Path
    - `IsValidFile/PathName` : check validation of file/path name, with `SearchValues`
    - `ReplaceInvalidFileNameChar` : Replace invalid file name characters
- Stream
    - `Read(Exactly)` : Read data into `unmanaged` span
    - `ReadExactlyIntoArray` : Read exactly data into an `unmanaged` array with specific length
    - `ReadWithInt32Prefix` : Read a `int` as array length, and do `ReadExactlyIntoArray`
- String
    - `CreateAsSpan` : Create string by <see cref="DefaultInterpolatedStringHandler"/>, directly return inner ReadOnlySpan without allocate string
    - `(Try)Unescape` : Unescape string
- Number
    - `IncAnd(Try)Wrap` : Increment the number, if the result is greater than given `max`, then wrap it
    - `Normalize` : Linear normalize value into [0,1]
    - `NormalizeUnclamped` : Linear normalize value into [0,1], but not clamped
    - `MapTo` : Linear map a value from [a, b] to [c, d], no clamp
    - `FlipNegative` : `if (value < 0) value = ~value`, useful on `BinarySearch` result
    - `Min/Max` : Overloads for `params ReadOnlySpan<>`
    - `MinMax(T, T)` : Reorder input 2 args
    - `MinMax(ROS<T>)` : Get min and max in one iteration
    - `GetCheckedOffset` : `Index.GetOffset` with overflow check
    - `ValidateSliceArgs` : Check if `start` and `length` is valid within a collection
- Geometry
    - `ToNormalized` : Normalize `Vector2/3`, `Quaternion`
    - `ToEulerAngles`
- Async
    - `GetAwaiter` : Support `await` keyword for `ValueTask?`, `ValueTask<>?`
    - `CatchCancallation` : Return a awaitable that will catch `TaskCancellationException`.
- Delegate
    - `Create` : Create delegate with an `object` and a static method. It is actually the way compiler use to create delegate for extension methods.
- Enum
    - `HasAnyFlag` : Check if a enum value has one of given flags.
- Random
    - `SelectWeight` : Weighted random
    - `NextSingle/Double` : Get a random float number in specific range
    - `NextBoolean` : Get a random boolean value
    - `NextItem` : Get a random item in collection
- Unsafe
    - `AsReadOnly` : Perform `Unsafe.As` for `ref readonly` variables
- Utils
    - `SetField` : If given value is not equals to field, set value and return `true`, otherwise return `false`
