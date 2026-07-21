namespace Trarizon.Library.Functional.Abstraction;

internal interface IOptional<out T>
{
    bool HasValue { get; }
    T Value { get; }
}
