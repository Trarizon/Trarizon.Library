using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;

namespace Trarizon.Library.Experimental.TaggingNavigation;
internal class TagresManager
{
    private HashSet<Item> _items;
    private PrefixTreeDictionary<string, Tag> _tags;

    public void AddTag(Tag tag)
    {
        string str="";
        str.AsSpan().Split("dd");
        
        _tags.GetOrAdd(tag.GetPathSplits(), tag);
    }

    public IEnumerable<Item> FilterByTag(string tagpath)
    {
        if (!_tags.TryGetNode(tagpath.Split('/'), out var node)) {
            return [];
        }

        return TraEnumerable.EnumerateDescendants(node, n => n.Children, includeSelf: true, depthFirst: true)
            .Where(n => n.IsEnd)
            .SelectMany(n => n.Value.TaggedItems, (_, col) => col)
            .Distinct();
    }
}
