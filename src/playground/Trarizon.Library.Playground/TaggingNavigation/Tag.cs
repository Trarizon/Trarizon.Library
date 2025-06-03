namespace Trarizon.Library.Experimental.TaggingNavigation;
public partial class Tag(string tagPath)
{
    private string _path = tagPath;
    private int _nameIndex;
    private int _priority;
    private List<Item> _items = [];
    private Tag? _parent;
    private IEnumerable<Tag>? _children;

    public ReadOnlySpan<char> Name => _path.AsSpan()[_nameIndex..];

    public ReadOnlySpan<char> Path => _path;

    public int Priority => _priority;

    public IEnumerable<Item> TaggedItems => _items;

    public Tag? Parent => _parent;

    public IEnumerable<Tag> Children => _children ??= [];

    public string[] GetPathSplits() => _path.Split('/');
}
