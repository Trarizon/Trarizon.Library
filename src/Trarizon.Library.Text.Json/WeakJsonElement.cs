using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Trarizon.Library.Text.Json.Internal;

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

    public T? AsNumber<T>() where T : struct
#if NET7_0_OR_GREATER
        , INumber<T>
#endif
    {
        if (Element.ValueKind is not JsonValueKind.Number)
            return null;

        if (typeof(T) == typeof(byte)) {
            var val = Element.GetByte();
            return Unsafe.As<byte, T>(ref val);
        }
        if (typeof(T) == typeof(sbyte)) {
            var val = Element.GetSByte();
            return Unsafe.As<sbyte, T>(ref val);
        }
        if (typeof(T) == typeof(short)) {
            var val = Element.GetInt16();
            return Unsafe.As<short, T>(ref val);
        }
        if (typeof(T) == typeof(ushort)) {
            var val = Element.GetUInt16();
            return Unsafe.As<ushort, T>(ref val);
        }
        if (typeof(T) == typeof(int)) {
            var val = Element.GetInt32();
            return Unsafe.As<int, T>(ref val);
        }
        if (typeof(T) == typeof(uint)) {
            var val = Element.GetUInt32();
            return Unsafe.As<uint, T>(ref val);
        }
        if (typeof(T) == typeof(long)) {
            var val = Element.GetInt64();
            return Unsafe.As<long, T>(ref val);
        }
        if (typeof(T) == typeof(ulong)) {
            var val = Element.GetUInt64();
            return Unsafe.As<ulong, T>(ref val);
        }
        if (typeof(T) == typeof(float)) {
            var val = Element.GetSingle();
            return Unsafe.As<float, T>(ref val);
        }
        if (typeof(T) == typeof(double)) {
            var val = Element.GetDouble();
            return Unsafe.As<double, T>(ref val);
        }
        if (typeof(T) == typeof(decimal)) {
            var val = Element.GetDecimal();
            return Unsafe.As<decimal, T>(ref val);
        }

        Throws.ThrowNotSupport("Unknown number type");
        return default!;

    }

    public bool TryAsNumber<T>(out T value) where T : struct
#if NET7_0_OR_GREATER
        , INumber<T>
#endif
    {
        if (Element.ValueKind is not JsonValueKind.Number) {
            value = default;
            return false;
        }

        if (typeof(T) == typeof(byte)) {
            bool res = Element.TryGetByte(out var val);
            value = Unsafe.As<byte, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(sbyte)) {
            bool res = Element.TryGetSByte(out var val);
            value = Unsafe.As<sbyte, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(short)) {
            bool res = Element.TryGetInt16(out var val);
            value = Unsafe.As<short, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(ushort)) {
            bool res = Element.TryGetUInt16(out var val);
            value = Unsafe.As<ushort, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(int)) {
            bool res = Element.TryGetInt32(out var val);
            value = Unsafe.As<int, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(uint)) {
            bool res = Element.TryGetUInt32(out var val);
            value = Unsafe.As<uint, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(long)) {
            bool res = Element.TryGetInt64(out var val);
            value = Unsafe.As<long, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(ulong)) {
            bool res = Element.TryGetUInt64(out var val);
            value = Unsafe.As<ulong, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(float)) {
            bool res = Element.TryGetSingle(out var val);
            value = Unsafe.As<float, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(double)) {
            bool res = Element.TryGetDouble(out var val);
            value = Unsafe.As<double, T>(ref val);
            return res;
        }
        if (typeof(T) == typeof(decimal)) {
            bool res = Element.TryGetDecimal(out var val);
            value = Unsafe.As<decimal, T>(ref val);
            return res;
        }

        value = default;
        Throws.ThrowNotSupport("Unknown number type");
        return default!;
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
