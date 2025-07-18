﻿using Microsoft.CodeAnalysis;
using Monads = Trarizon.Library.Functional;

namespace Trarizon.Library.Roslyn.Extensions;
public static class UtilsExt
{
    public static Monads.Optional<T> ToTraOptional<T>(this Optional<T> optional)
        => optional.HasValue ? Monads.Optional.Of(optional.Value) : default;
}
