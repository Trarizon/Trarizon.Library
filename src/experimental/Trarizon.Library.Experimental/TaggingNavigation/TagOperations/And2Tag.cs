namespace Trarizon.Library.Experimental.TaggingNavigation.TagOperations;
public sealed class And2Tag : Tag
{
    private Tag _left;
    private Tag _right;

    internal And2Tag(Tag left, Tag right)
    {
        _left = left;
        _right = right;
    }

    public override bool MatchTag(ReadOnlySpan<string> tags)
    {
        return _left.MatchTag(tags) && _right.MatchTag(tags);
    }
}
