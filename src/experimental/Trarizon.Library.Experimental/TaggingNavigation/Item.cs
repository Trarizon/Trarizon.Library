namespace Trarizon.Library.Experimental.TaggingNavigation;
public class Item(DirectoryMonitor monitor, string path)
{
    private List<Tag> _tags = new();
    private List<ValueTag> _valueTags = new();

    public string Name
    {
        get => field ??= System.IO.Path.GetFileName(Path);
        set => field = value;
    }

    public string Path { get; private set; } = path;

    public ICollection<Tag> Tags => _tags;

    public ICollection<ValueTag> ValueTags => _valueTags;
}
