using System.Collections;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Trash.StringTemplating;
[InterpolatedStringHandler]
public sealed class StringTemplate
{
    private readonly List<object?> _list;

    private readonly IndexCollection<string> _literals;
    private readonly IndexCollection<object?> _formatteds;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060")]
    public StringTemplate(int literalLength, int formattedCount)
    {
        _list = new(formattedCount * 2);
        _literals = new(this, 0);
        _formatteds = new(this, 1);
    }

    public IReadOnlyList<string> Literals => _literals;

    public IReadOnlyList<object?> Formatteds => _formatteds;

    public void AppendLiteral(string str)
    {
        _literals.Indices.Add(_list.Count);
        _list.Add(str);
    }

    public void AppendFormatted<T>(T obj)
    {
        _formatteds.Indices.Add(_list.Count);
        _list.Add(obj);
    }

    private sealed class IndexCollection<T>(StringTemplate template, int capacity) : IReadOnlyList<T>
    {
        public readonly List<int> Indices = new(capacity);

        public T this[int index] => (T)template._list[Indices[index]]!;

        public int Count => Indices.Count;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var index in Indices) {
                yield return (T)template._list[index]!;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
