using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Trarizon.Library.Roslyn.Pipeline.Collections;

namespace Trarizon.Library.Roslyn.Pipeline;

public sealed record DiagnosticData
{
    public DiagnosticDescriptor Descriptor { get; }
    public string? FilePath { get; }
    public TextSpan TextSpan { get; }
    public LinePositionSpan LineSpan { get; }
    public SequenceEquatableImmutableArray<object?> MessageArgs { get; }

    private DiagnosticData(DiagnosticDescriptor descriptor, string? filePath, TextSpan textSpan, LinePositionSpan lineSpan, SequenceEquatableImmutableArray<object?> messageArgs)
    {
        Descriptor = descriptor;
        FilePath = filePath;
        TextSpan = textSpan;
        LineSpan = lineSpan;
        MessageArgs = messageArgs;
    }

    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, string.IsNullOrEmpty(FilePath) ? null : Location.Create(FilePath!, TextSpan, LineSpan), ImmutableCollectionsMarshal.AsArray(MessageArgs.Array));

    public DiagnosticData(DiagnosticDescriptor descriptor, SyntaxNode? syntax, ImmutableArray<object?> messageArgs) :
        this(descriptor, syntax?.SyntaxTree?.FilePath, syntax?.Span ?? default, syntax?.SyntaxTree?.GetLineSpan(syntax.Span).Span ?? default, messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, SyntaxNode? syntax, params ReadOnlySpan<object?> messageArgs) :
        this(descriptor, syntax, messageArgs.ToImmutableArray())
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxToken? syntax, ImmutableArray<object?> messageArgs) :
        this(descriptor, syntax?.SyntaxTree?.FilePath, syntax?.Span ?? default, syntax is not { } syn ? default : syn.SyntaxTree?.GetLineSpan(syn.Span).Span ?? default, messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxToken? syntax, params ReadOnlySpan<object?> messageArgs) :
        this(descriptor, syntax, messageArgs.ToSequenceEquatableImmutableArray())
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, SyntaxReference? syntax, ImmutableArray<object?> messageArgs) :
        this(descriptor, syntax?.GetSyntax(), messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, SyntaxReference? syntax, params ReadOnlySpan<object?> messageArgs) :
        this(descriptor, syntax?.GetSyntax(), messageArgs)
    { }
}
