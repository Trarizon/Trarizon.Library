using System.Collections;
using Trarizon.TextCommanding.Exceptions;
using Trarizon.TextCommanding.Utility;

namespace Trarizon.TextCommanding.Input;
internal static class RawInputParser
{
    public static object? Parse(Type type, string text)
    {
        if (type == typeof(string))
            return text;
        if (Nullable.GetUnderlyingType(type) is Type nullableUnderlyingType)
            type = nullableUnderlyingType;
        if (ReflectionUtil.TryParseText(type, text, out var res))
            return res;
        ThrowHelper.TextCommandInitializeFailed($"Parameter type must be string or IParsable<> or has Parse(string) method");
        return default;
    }

    public static object ParseArray(Type elementType, ReadOnlySpan<string> texts)
    {
        if (texts.Length == 0)
            return ReflectionUtil.GetEmptyArray(elementType);

        Array array = Array.CreateInstance(elementType, texts.Length);
        for (int i = 0; i < texts.Length; i++) {
            array.SetValue(Parse(elementType, texts[i]), i);
        }
        return array;
    }

    public static object ParseList(Type elementType, ReadOnlySpan<string> texts)
    {
        IList list = (IList)Activator.CreateInstance(ReflectionUtil.GetGenericListType(elementType))!;
        for (int i = 0; i < texts.Length; i++) {
            list.Add(Parse(elementType, texts[i]));
        }
        return list;
    }
}
