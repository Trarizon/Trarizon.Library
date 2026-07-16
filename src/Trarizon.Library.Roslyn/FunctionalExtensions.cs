using Microsoft.CodeAnalysis;
using Monads = Trarizon.Library.Functional;

namespace Trarizon.Library.Roslyn;
public static class FunctionalExtensions
{
    public static Monads.Optional<T> ToTraOptional<T>(this Optional<T> optional)
        => optional.HasValue ? Monads.Optional.Of(optional.Value) : default;
}
