namespace Trarizon.Library.Functional.Unions;

internal interface ITaggedUnion
{
    string ToString(bool includeVariantInfo);
}
