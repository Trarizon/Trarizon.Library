using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;

namespace Trarizon.Library.Roslyn.Diagnostics;
public sealed record DiagnosticData(
    DiagnosticDescriptor Descriptor,
    Location? Location,
    params object?[]? MessageArgs)
    : IEnumerable<DiagnosticData>, ICollection<DiagnosticData>,IReadOnlyCollection<DiagnosticData>
{
    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, MessageArgs);

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxNode? syntax, params object?[]? messageArgs) :
        this(descriptor, syntax?.GetLocation(), messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxToken syntax, params object?[]? messageArgs) :
        this(descriptor, syntax.GetLocation(), messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxToken? syntax, params object?[]? messageArgs) :
        this(descriptor, syntax?.GetLocation(), messageArgs)
    { }

    public DiagnosticData(DiagnosticDescriptor descriptor, in SyntaxReference? syntax, params object?[]? messageArgs) :
        this(descriptor, syntax?.GetSyntax().GetLocation(), messageArgs)
    { }


    int ICollection<DiagnosticData>.Count => 1;

    bool ICollection<DiagnosticData>.IsReadOnly => true;

    int IReadOnlyCollection<DiagnosticData>.Count => 1;

    IEnumerator<DiagnosticData> IEnumerable<DiagnosticData>.GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    void ICollection<DiagnosticData>.Add(DiagnosticData item) => throw new System.NotSupportedException();
    void ICollection<DiagnosticData>.Clear() => throw new System.NotSupportedException();
    bool ICollection<DiagnosticData>.Contains(DiagnosticData item) => item == this;
    void ICollection<DiagnosticData>.CopyTo(DiagnosticData[] array, int arrayIndex) => array[arrayIndex] = this;
    bool ICollection<DiagnosticData>.Remove(DiagnosticData item) => throw new System.NotSupportedException();

    private struct Enumerator : IEnumerator<DiagnosticData>
    {
        private readonly DiagnosticData _data;
        private bool _isCurrent = false;

        internal Enumerator(DiagnosticData data)
        {
            _data = data;
        }

        public readonly DiagnosticData Current => _isCurrent ? _data : null!;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (!_isCurrent) {
                _isCurrent = true;
                return true;
            }
            return false;
        }

        public void Reset() { _isCurrent = false; }
    }
}
