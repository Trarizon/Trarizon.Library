namespace Trarizon.Library;
public static class TraUtils
{
    public static bool SetField<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) {
            return false;
        }
        field = value;
        return true;
    }

    public static bool SetField<T>(ref T field, T value, out T oldValue)
    {
        oldValue = field;
        return SetField(ref field, value);
    }
}
