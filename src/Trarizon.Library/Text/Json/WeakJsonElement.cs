#if !NETSTANDARD

using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Trarizon.Library.Wrappers;

namespace Trarizon.Library.Text.Json;
/// <summary>
/// Wraps <see cref="JsonValueKind"/> checking, if current kind is not expected,
/// return values will be null
/// </summary>
/// <param name="element"></param>
public readonly struct WeakJsonElement(JsonElement element)
{
    public readonly JsonElement Element = element;

    public WeakJsonElement? this[int index]
        => Element.ValueKind is JsonValueKind.Array ? new(Element[index]) : null;

    public WeakJsonElement? this[ReadOnlySpan<char> propertyName]
        => Element.ValueKind is JsonValueKind.Object ? new(Element.GetProperty(propertyName)) : null;

    public WeakJsonElement? this[ReadOnlySpan<byte> utf8PropertyName]
        => Element.ValueKind is JsonValueKind.Object ? new(Element.GetProperty(utf8PropertyName)) : null;

    public int? ArrayLength => Element.ValueKind is JsonValueKind.Array ? Element.GetArrayLength() : null;

    public bool IsNullValue => Element.ValueKind is JsonValueKind.Null;

    public bool TryGetElementAt(int index, out WeakJsonElement value)
    {
        if (index >= 0 && Element.ValueKind is JsonValueKind.Array && index < Element.GetArrayLength()) {
            value = new(Element[index]);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetProperty(ReadOnlySpan<char> propertyName, out WeakJsonElement value)
    {
        if (Element.ValueKind is JsonValueKind.Object) {
            var res = Element.TryGetProperty(propertyName, out var jvalue);
            value = new(jvalue);
            return res;
        }
        value = default;
        return false;
    }

    public string? AsString()
        => Element.ValueKind is JsonValueKind.String ? Element.GetString() : null;

    public string? AsString(out bool existsAndValid)
    {
        if (Element.ValueKind is JsonValueKind.String or JsonValueKind.Null) {
            existsAndValid = true;
            return Element.GetString();
        }
        else {
            existsAndValid = false;
            return default;
        }
    }

    public bool? AsBoolean() => Element.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };

    public bool? AsNullableBoolean(out bool existsAndValid)
    {
        switch (Element.ValueKind) {
            case JsonValueKind.True:
                existsAndValid = true;
                return true;
            case JsonValueKind.False:
                existsAndValid = true;
                return false;
            case JsonValueKind.Null:
                existsAndValid = true;
                return null;
            default:
                existsAndValid = false;
                return null;
        }
    }

    public T? AsNumber<T>() where T : struct, INumber<T>
    {
        if (Element.ValueKind is not JsonValueKind.Number)
            return null;

        if (typeof(T) == typeof(byte)) return Unsafe.BitCast<byte, T>(Element.GetByte());
        if (typeof(T) == typeof(sbyte)) return Unsafe.BitCast<sbyte, T>(Element.GetSByte());
        if (typeof(T) == typeof(short)) return Unsafe.BitCast<short, T>(Element.GetInt16());
        if (typeof(T) == typeof(ushort)) return Unsafe.BitCast<ushort, T>(Element.GetUInt16());
        if (typeof(T) == typeof(int)) return Unsafe.BitCast<int, T>(Element.GetInt32());
        if (typeof(T) == typeof(uint)) return Unsafe.BitCast<uint, T>(Element.GetUInt32());
        if (typeof(T) == typeof(long)) return Unsafe.BitCast<long, T>(Element.GetInt64());
        if (typeof(T) == typeof(ulong)) return Unsafe.BitCast<ulong, T>(Element.GetUInt64());
        if (typeof(T) == typeof(float)) return Unsafe.BitCast<float, T>(Element.GetSingle());
        if (typeof(T) == typeof(double)) return Unsafe.BitCast<double, T>(Element.GetDouble());
        if (typeof(T) == typeof(decimal)) return Unsafe.BitCast<decimal, T>(Element.GetDecimal());

        return ThrowHelper.ThrowNotSupportedException<T?>("Unknown number type");
    }

    public bool TryAsNumber<T>(out T value) where T : struct, INumber<T>
    {
        if (Element.ValueKind is not JsonValueKind.Number) {
            value = default;
            return false;
        }

        if (typeof(T) == typeof(byte)) {
            bool res = Element.TryGetByte(out var val);
            value = Unsafe.BitCast<byte, T>(val);
            return res;
        }
        if (typeof(T) == typeof(sbyte)) {
            bool res = Element.TryGetSByte(out var val);
            value = Unsafe.BitCast<sbyte, T>(val);
            return res;
        }
        if (typeof(T) == typeof(short)) {
            bool res = Element.TryGetInt16(out var val);
            value = Unsafe.BitCast<short, T>(val);
            return res;
        }
        if (typeof(T) == typeof(ushort)) {
            bool res = Element.TryGetUInt16(out var val);
            value = Unsafe.BitCast<ushort, T>(val);
            return res;
        }
        if (typeof(T) == typeof(int)) {
            bool res = Element.TryGetInt32(out var val);
            value = Unsafe.BitCast<int, T>(val);
            return res;
        }
        if (typeof(T) == typeof(uint)) {
            bool res = Element.TryGetUInt32(out var val);
            value = Unsafe.BitCast<uint, T>(val);
            return res;
        }
        if (typeof(T) == typeof(long)) {
            bool res = Element.TryGetInt64(out var val);
            value = Unsafe.BitCast<long, T>(val);
            return res;
        }
        if (typeof(T) == typeof(ulong)) {
            bool res = Element.TryGetUInt64(out var val);
            value = Unsafe.BitCast<ulong, T>(val);
            return res;
        }
        if (typeof(T) == typeof(float)) {
            bool res = Element.TryGetSingle(out var val);
            value = Unsafe.BitCast<float, T>(val);
            return res;
        }
        if (typeof(T) == typeof(double)) {
            bool res = Element.TryGetDouble(out var val);
            value = Unsafe.BitCast<double, T>(val);
            return res;
        }
        if (typeof(T) == typeof(decimal)) {
            bool res = Element.TryGetDecimal(out var val);
            value = Unsafe.BitCast<decimal, T>(val);
            return res;
        }

        value = default;
        return ThrowHelper.ThrowNotSupportedException<bool>("Unknown number type");
    }

    public DateTime? GetDateTime() => Element.ValueKind is JsonValueKind.String ? Element.GetDateTime() : null;

    public bool TryGetDateTime(out DateTime value)
    {
        if (Element.ValueKind is JsonValueKind.String) {
            return Element.TryGetDateTime(out value);
        }
        value = default;
        return false;
    }

    public DateTimeOffset? GetDateTimeOffset() => Element.ValueKind is JsonValueKind.String ? Element.GetDateTimeOffset() : null;

    public bool TryGetDateTimeOffset(out DateTimeOffset value)
    {
        if (Element.ValueKind is JsonValueKind.String) {
            return Element.TryGetDateTimeOffset(out value);
        }
        value = default;
        return false;
    }

    public Guid? GetGuid() => Element.ValueKind is JsonValueKind.String ? Element.GetGuid() : null;

    public bool TryGetGuid(out Guid value)
    {
        if (Element.ValueKind is JsonValueKind.String) {
            return Element.TryGetGuid(out value);
        }
        value = default;
        return false;
    }

    public byte[]? GetBytesFromBase64() => Element.ValueKind is JsonValueKind.String ? Element.GetBytesFromBase64() : null;

    public bool TryGetBytesFromBase64([NotNullWhen(true)] out byte[]? value)
    {
        if (Element.ValueKind is JsonValueKind.String) {
            return Element.TryGetBytesFromBase64(out value);
        }
        value = default;
        return false;
    }
}

#endif
