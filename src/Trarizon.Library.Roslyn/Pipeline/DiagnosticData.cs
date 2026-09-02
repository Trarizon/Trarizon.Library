using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Runtime.InteropServices;

namespace Trarizon.Library.Roslyn.Pipeline;

public readonly record struct DiagnosticData
{
    public DiagnosticDescriptor Descriptor { get; }
    public string? FilePath { get; }
    public TextSpan TextSpan { get; }
    public LinePositionSpan LineSpan { get; }
    public EquatableImmutableArray<object?> MessageArgs { get; }

    private DiagnosticData(DiagnosticDescriptor descriptor, string? filePath, TextSpan textSpan, LinePositionSpan lineSpan, EquatableImmutableArray<object?> messageArgs)
    {
        Descriptor = descriptor;
        FilePath = filePath;
        TextSpan = textSpan;
        LineSpan = lineSpan;
        MessageArgs = messageArgs;
    }

    public Diagnostic ToDiagnostic() => Diagnostic.Create(
        Descriptor, string.IsNullOrEmpty(FilePath) ? null : Location.Create(FilePath!, TextSpan, LineSpan),
#if IMMUTABLE_MARSHAL
        ImmutableCollectionsMarshal.AsArray(MessageArgs.Array)
#else
        MessageArgs.ToArray()
#endif
    );

    public DiagnosticData(DiagnosticDescriptor descriptor, SyntaxNode? syntax, params EquatableImmutableArray<object?> messageArgs) :
        this(descriptor, syntax?.SyntaxTree?.FilePath, syntax?.Span ?? default, syntax?.SyntaxTree?.GetLineSpan(syntax.Span).Span ?? default, messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxToken? syntax, params EquatableImmutableArray<object?> messageArgs) :
        this(descriptor, syntax?.SyntaxTree?.FilePath, syntax?.Span ?? default, syntax is not { } syn ? default : syn.SyntaxTree?.GetLineSpan(syn.Span).Span ?? default, messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, SyntaxReference? syntax, params EquatableImmutableArray<object?> messageArgs) :
        this(descriptor, syntax?.GetSyntax(), messageArgs)
    { }
}
