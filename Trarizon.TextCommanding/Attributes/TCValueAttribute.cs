namespace Trarizon.TextCommanding.Attributes;
public class TCValueAttribute(int order = 0) : TCParameterAttribute
{
    public int Order => order;
}
