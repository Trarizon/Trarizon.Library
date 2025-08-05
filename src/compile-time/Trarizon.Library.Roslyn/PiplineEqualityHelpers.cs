using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using Trarizon.Library.Roslyn.Collections;

namespace Trarizon.Library.Roslyn;
public static partial class PiplineEqualityHelpers
{
    public static SequenceEquatableImmutableArray<T> ToSequenceEquatableImmutableArray<T>(this IEnumerable<T> source)
        => new(source.ToImmutableArray());

    public static SequenceEquatableCollection<T[], T> WrapAsPiplineEquatable<T>(this T[] list) => new(list);

    // Get a Location object that doesn't store a reference to the compilation.
    public static Location ToPiplineEquatableLocation(this Location location)
    {
        return Location.Create(location.SourceTree?.FilePath ?? "", location.SourceSpan, location.GetLineSpan().Span);
    }
}
