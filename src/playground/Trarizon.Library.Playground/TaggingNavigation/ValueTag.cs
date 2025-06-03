namespace Trarizon.Library.Experimental.TaggingNavigation;
public class ValueTag(string path, object value) : Tag(path)
{
    public object Value { get; } = value;
}
