namespace Trarizon.Library.Functional.Unions;

internal interface IOptional<out T>
{
    bool HasValue { get; }
    T Value { get; }
}
