namespace Trarizon.Library.Experimental.TaggingNavigation.TagFilters;
public readonly struct TagFilter(Tag tag) : ITagFilter
{
    public bool MatchTag(Item item) => item.Tags.Contains(tag);
}

public readonly struct NotOpTagFilter<TOperand>(TOperand tag) : ITagFilter
    where TOperand : ITagFilter
{
    public bool MatchTag(Item item) => !tag.MatchTag(item);
}

public readonly struct And2OpTagFilter<TL, TR>(TL left, TR right) : ITagFilter
    where TL : ITagFilter where TR : ITagFilter
{
    public bool MatchTag(Item item) => left.MatchTag(item) && right.MatchTag(item);
}

public readonly struct Or2OpTagFilter<TL, TR>(TL left, TR right) : ITagFilter
    where TL : ITagFilter where TR : ITagFilter
{
    public bool MatchTag(Item item) => left.MatchTag(item) || right.MatchTag(item);
}
