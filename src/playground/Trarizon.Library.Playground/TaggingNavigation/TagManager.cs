using Trarizon.Library.Collections.Generic;

namespace Trarizon.Library.Experimental.TaggingNavigation;
internal class TagManager
{
    private PrefixTreeDictionary_old<string, Tag> _tags;

    public TagManager()
    {
        _tags = new();
    }

    public void AddTag(Tag tag)
    {
        _tags.GetOrAdd(tag.GetPathSplits(), tag);
    }

    public PrefixTreeDictionary_old<string, Tag>.Node? GetTagNode(string path)
    {
        if (_tags.TryGetNode(path.Split('/'), out var node)) {
            return node;
        }
        return null;
    }
}
