using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Trarizon.Library.Roslyn.Collections;

namespace Trarizon.Library.Roslyn.Diagnostics;
public sealed record DiagnosticData
{
    public DiagnosticDescriptor Descriptor { get; private init; }
    public Location? Location { get; private init; }
    public SequenceEquatableImmutableArray<object?> MessageArgs { get; private init; }

    private DiagnosticData(DiagnosticDescriptor descriptor, Location? location, SequenceEquatableImmutableArray<object?> messageArgs)
    {
        Descriptor = descriptor;
        Location = location;
        MessageArgs = messageArgs;
    }

    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, ImmutableCollectionsMarshal.AsArray(MessageArgs.Array));

    public DiagnosticData(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) :
        this(descriptor, location?.ToPiplineEquatableLocation(), messageArgs?.ToSequenceEquatableImmutableArray() ?? new(ImmutableArray<object?>.Empty))
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxNode? syntax, params object?[]? messageArgs) :
        this(descriptor, syntax?.GetLocation()?.ToPiplineEquatableLocation(), messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxToken syntax, params object?[]? messageArgs) :
        this(descriptor, syntax.GetLocation()?.ToPiplineEquatableLocation(), messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxToken? syntax, params object?[]? messageArgs) :
        this(descriptor, syntax?.GetLocation()?.ToPiplineEquatableLocation(), messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxReference? syntax, params object?[]? messageArgs) :
        this(descriptor, syntax?.GetSyntax()?.GetLocation()?.ToPiplineEquatableLocation(), messageArgs)
    { }
}
