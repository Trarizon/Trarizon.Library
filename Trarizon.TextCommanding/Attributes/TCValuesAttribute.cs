namespace Trarizon.TextCommanding.Attributes;
public sealed class TCValuesAttribute(int order = 0, int maxCount = -1) : TCValueAttribute(order)
{
    public int MaxCount => maxCount;
}
